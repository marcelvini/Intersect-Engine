using System;
using System.Collections.Generic;
using System.Linq;
using Intersect.Client.Core;
using Intersect.Client.Core.Controls;
using Intersect.Client.Entities.Events;
using Intersect.Client.Entities.Projectiles;
using Intersect.Client.Framework.Entities;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Items;
using Intersect.Client.General;
using Intersect.Client.Interface.Game;
using Intersect.Client.Interface.Game.Chat;
using Intersect.Client.Interface.Game.EntityPanel;
using Intersect.Client.Localization;
using Intersect.Client.Maps;
using Intersect.Client.Networking;
using Intersect.Config.Guilds;
using Intersect.Configuration;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Maps;
using Intersect.Network.Packets.Server;
using Intersect.Utilities;
using MapAttribute = Intersect.Enums.MapAttribute;

namespace Intersect.Client.Entities
{

    public partial class Player : Entity, IPlayer
    {

        public delegate void InventoryUpdated();

        private Guid _class;

        public Guid Class
        {
            get => _class;
            set
            {
                if (_class == value)
                {
                    return;
                }

                _class = value;
                LoadAnimationTexture(Sprite ?? TransformedSprite, SpriteAnimations.Attack);
            }
        }

        public Access AccessLevel { get; set; }

        public long Experience { get; set; } = 0;

        public long ExperienceToNextLevel { get; set; } = 0;

        IReadOnlyList<IFriendInstance> IPlayer.Friends => Friends;

        public List<IFriendInstance> Friends { get; set; } = new List<IFriendInstance>();

        IReadOnlyList<IHotbarInstance> IPlayer.HotbarSlots => Hotbar.ToList();

        public HotbarInstance[] Hotbar { get; set; } = new HotbarInstance[Options.Instance.PlayerOpts.HotbarSlotCount];

        public InventoryUpdated InventoryUpdatedDelegate { get; set; }

        IReadOnlyDictionary<Guid, long> IPlayer.ItemCooldowns => ItemCooldowns;

        public Dictionary<Guid, long> ItemCooldowns { get; set; } = new Dictionary<Guid, long>();

        private Entity mLastBumpedEvent = null;

        private List<IPartyMember> mParty;

        IReadOnlyDictionary<Guid, QuestProgress> IPlayer.QuestProgress => QuestProgress;

        public Dictionary<Guid, QuestProgress> QuestProgress { get; set; } = new Dictionary<Guid, QuestProgress>();

        public Guid[] HiddenQuests { get; set; } = new Guid[0];

        IReadOnlyDictionary<Guid, long> IPlayer.SpellCooldowns => SpellCooldowns;

        public Dictionary<Guid, long> SpellCooldowns { get; set; } = new Dictionary<Guid, long>();

        public int StatPoints { get; set; } = 0;

        public EntityBox TargetBox { get; set; }

        public Guid TargetIndex { get; set; }

        TargetType IPlayer.TargetType => (TargetType)TargetType;

        public int TargetType { get; set; }

        public long CombatTimer { get; set; } = 0;

        public long IsCastingCheckTimer { get; set; }

        // Target data
        private long mlastTargetScanTime = 0;

        Guid mlastTargetScanMap = Guid.Empty;

        Point mlastTargetScanLocation = new Point(-1, -1);

        Dictionary<Entity, TargetInfo> mlastTargetList = new Dictionary<Entity, TargetInfo>(); // Entity, Last Time Selected

        Entity mLastEntitySelected = null;

        private Dictionary<int, long> mLastHotbarUseTime = new Dictionary<int, long>();
        private int mHotbarUseDelay = 150;

        /// <summary>
        /// Name of our guild if we are in one.
        /// </summary>
        public string Guild { get; set; }

        string IPlayer.GuildName => Guild;

        /// <summary>
        /// Index of our rank where 0 is the leader
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Returns whether or not we are in a guild by checking to see if we are assigned a guild name
        /// </summary>
        public bool IsInGuild => !string.IsNullOrWhiteSpace(Guild);

        /// <summary>
        /// Obtains our rank and permissions from the game config
        /// </summary>
        public GuildRank GuildRank => IsInGuild ? Options.Instance.Guild.Ranks[Math.Max(0, Math.Min(Rank, Options.Instance.Guild.Ranks.Length - 1))] : null;

        /// <summary>
        /// Contains a record of all members of this player's guild.
        /// </summary>
        public GuildMember[] GuildMembers = new GuildMember[0];

        public Player(Guid id, PlayerEntityPacket packet) : base(id, packet, EntityType.Player)
        {
            for (var i = 0; i < Options.Instance.PlayerOpts.HotbarSlotCount; i++)
            {
                Hotbar[i] = new HotbarInstance();
            }

            mRenderPriority = 2;
        }

        IReadOnlyList<IPartyMember> IPlayer.PartyMembers => Party;

        public List<IPartyMember> Party
        {
            get
            {
                if (mParty == null)
                {
                    mParty = new List<IPartyMember>();
                }

                return mParty;
            }
        }

        public override Guid MapId
        {
            get => base.MapId;
            set
            {
                if (value != base.MapId)
                {
                    var oldMap = Maps.MapInstance.Get(base.MapId);
                    var newMap = Maps.MapInstance.Get(value);
                    base.MapId = value;
                    if (Globals.Me == this)
                    {
                        if (Maps.MapInstance.Get(Globals.Me.MapId) != null)
                        {
                            Audio.PlayMusic(Maps.MapInstance.Get(Globals.Me.MapId).Music, ClientConfiguration.Instance.MusicFadeTimer, ClientConfiguration.Instance.MusicFadeTimer, true);
                        }

                        if (newMap != null && oldMap != null)
                        {
                            newMap.CompareEffects(oldMap);
                        }
                    }
                }
            }
        }

        public bool IsFriend(IPlayer player)
        {
            return Friends.Any(
                friend => player != null &&
                          string.Equals(player.Name, friend.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool IsGuildMate(IPlayer player)
        {
            return GuildMembers.Any(
                guildMate =>
                    player != null &&
                    string.Equals(player.Name, guildMate.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        bool IPlayer.IsInParty => IsInParty();

        public bool IsInParty()
        {
            return Party.Count > 0;
        }

        public bool IsInMyParty(IPlayer player) => player != null && IsInMyParty(player.Id);

        public bool IsInMyParty(Guid id) => Party.Any(member => member.Id == id);

        public bool IsBusy => !(Globals.EventHolds.Count == 0 &&
                     !Globals.MoveRouteActive &&
                     Globals.GameShop == null &&
                     Globals.InBank == false &&
                     Globals.InCraft == false &&
                     Globals.InTrade == false &&
                     !Interface.Interface.HasInputFocus());

        public override bool Update()
        {

            if (Globals.Me == this)
            {
                HandleInput();
            }


            if (!IsBusy)
            {
                if (this == Globals.Me && IsMoving == false)
                {
                    ProcessDirectionalInput();
                }

                if (Controls.KeyDown(Control.AttackInteract))
                {
                    if (IsCasting)
                    {
                        if (IsCastingCheckTimer < Timing.Global.Milliseconds &&
                            Options.Combat.EnableCombatChatMessages)
                        {
                            ChatboxMsg.AddMessage(new ChatboxMsg(Strings.Combat.AttackWhileCastingDeny,
                                CustomColors.Alerts.Declined, ChatMessageType.Combat));
                            IsCastingCheckTimer = Timing.Global.Milliseconds + 350;
                        }
                    }
                    else if (!Globals.Me.TryAttack())
                    {
                        if (!Globals.Me.IsAttacking)
                        {
                            Globals.Me.AttackTimer = Timing.Global.Milliseconds + Globals.Me.CalculateAttackTime();
                        }
                    }
                }

                //Holding block button for "auto blocking"
                if (Controls.KeyDown(Control.Block))
                {
                    TryBlock();
                }
            }

            if (TargetBox == default && this == Globals.Me && Interface.Interface.GameUi != default)
            {
                // If for WHATEVER reason the box hasn't been created, create it.
                TargetBox = new EntityBox(Interface.Interface.GameUi.GameCanvas, EntityType.Player, null);
                TargetBox.Hide();
            }
            else if (TargetIndex != default)
            {
                if (!Globals.Entities.TryGetValue(TargetIndex, out var foundEntity))
                {
                    foundEntity = TargetBox.MyEntity?.MapInstance?.Entities.FirstOrDefault(entity => entity.Id == TargetIndex) as Entity;
                }

                if (foundEntity == default || foundEntity.IsHidden || foundEntity.IsStealthed)
                {
                    ClearTarget();
                }
            }

            TargetBox?.Update();

            // Hide our Guild window if we're not in a guild!
            if (this == Globals.Me && string.IsNullOrEmpty(Guild) && Interface.Interface.GameUi != null)
            {
                Interface.Interface.GameUi.HideGuildWindow();
            }

            var returnval = base.Update();

            return returnval;
        }

        //Loading
        public override void Load(EntityPacket packet)
        {
            base.Load(packet);
            var playerPacket = (PlayerEntityPacket)packet;
            Gender = playerPacket.Gender;
            Class = playerPacket.ClassId;
            AccessLevel = playerPacket.AccessLevel;
            CombatTimer = playerPacket.CombatTimeRemaining + Timing.Global.Milliseconds;
            Guild = playerPacket.Guild;
            Rank = playerPacket.GuildRank;

            HideAllPaperdolls = playerPacket.HideAllPaperdolls;

            if (playerPacket.Equipment != null)
            {
                if (this == Globals.Me && playerPacket.Equipment.InventorySlots != null)
                {
                    MyEquipment = playerPacket.Equipment.InventorySlots;
                }
                else if (playerPacket.Equipment.ItemIds != null)
                {
                    Equipment = playerPacket.Equipment.ItemIds;
                }
            }

            if (this == Globals.Me && TargetBox == null && Interface.Interface.GameUi != null)
            {
                TargetBox = new EntityBox(Interface.Interface.GameUi.GameCanvas, EntityType.Player, null);
                TargetBox.Hide();
            }
        }

        //Item Processing
        public void SwapItems(int fromSlotIndex, int toSlotIndex)
        {
            PacketSender.SendSwapInvItems(fromSlotIndex, toSlotIndex);
            var fromSlot = Inventory[fromSlotIndex];
            var toSlot = Inventory[toSlotIndex];

            if (
                fromSlot.ItemId == toSlot.ItemId
                && ItemBase.TryGet(toSlot.ItemId, out var itemInSlot)
                && itemInSlot.IsStackable
                && fromSlot.Quantity < itemInSlot.MaxInventoryStack
                && toSlot.Quantity < itemInSlot.MaxInventoryStack
            )
            {
                var combinedQuantity = fromSlot.Quantity + toSlot.Quantity;
                var toQuantity = Math.Min(itemInSlot.MaxInventoryStack, combinedQuantity);
                var fromQuantity = combinedQuantity - toQuantity;
                toSlot.Quantity = toQuantity;
                fromSlot.Quantity = fromQuantity;
                if (fromQuantity < 1)
                {
                    Inventory[fromSlotIndex].ItemId = default;
                }
            }
            else
            {
                Inventory[fromSlotIndex] = toSlot;
                Inventory[toSlotIndex] = fromSlot;
            }
        }

        public void TryDropItem(int inventorySlotIndex)
        {
            var inventorySlot = Inventory[inventorySlotIndex];
            if (!ItemBase.TryGet(inventorySlot.ItemId, out var itemDescriptor))
            {
                return;
            }

            var quantity = inventorySlot.Quantity;
            var canDropMultiple = quantity > 1;
            var inputType = canDropMultiple ? InputBox.InputType.NumericSliderInput : InputBox.InputType.YesNo;
            var prompt = canDropMultiple ? Strings.Inventory.DropItemPrompt : Strings.Inventory.DropPrompt;
            InputBox.Open(
                title: Strings.Inventory.DropItemTitle,
                prompt: prompt.ToString(itemDescriptor.Name),
                modal: true,
                inputType: inputType,
                onSuccess: DropInputBoxOkay,
                onCancel: default,
                userData: inventorySlotIndex,
                quantity: quantity,
                maxQuantity: quantity
            );
        }

        private void DropInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                PacketSender.SendDropItem((int)((InputBox)sender).UserData, value);
            }
        }

        public int FindItem(Guid itemId, int itemVal = 1)
        {
            for (var i = 0; i < Options.MaxInvItems; i++)
            {
                if (Inventory[i].ItemId == itemId && Inventory[i].Quantity >= itemVal)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetQuantityOfItemIn(IEnumerable<IItem> items, Guid itemId)
        {
            long count = 0;

            foreach (var slot in (items ?? Enumerable.Empty<IItem>()))
            {
                if (slot?.ItemId == itemId)
                {
                    count += slot.Quantity;
                }
            }

            return (int)Math.Min(count, int.MaxValue);
        }

        public int GetQuantityOfItemInBag(Guid itemId) => GetQuantityOfItemIn(Globals.Bag, itemId);

        public int GetQuantityOfItemInBank(Guid itemId) => GetQuantityOfItemIn(Globals.Bank, itemId);

        public int GetQuantityOfItemInInventory(Guid itemId) => GetQuantityOfItemIn(Inventory, itemId);

        public void TryUseItem(int index)
        {
            if (!IsItemOnCooldown(index) &&
                index >= 0 && index < Globals.Me.Inventory.Length && Globals.Me.Inventory[index]?.Quantity > 0)
            {
                PacketSender.SendUseItem(index, TargetIndex);
            }
        }

        public long GetItemCooldown(Guid id)
        {
            if (ItemCooldowns.ContainsKey(id))
            {
                return ItemCooldowns[id];
            }

            return 0;
        }

        public int FindHotbarItem(IHotbarInstance hotbarInstance)
        {
            var bestMatch = -1;

            if (hotbarInstance.ItemOrSpellId != Guid.Empty)
            {
                for (var i = 0; i < Inventory.Length; i++)
                {
                    var itm = Inventory[i];
                    if (itm != null && itm.ItemId == hotbarInstance.ItemOrSpellId)
                    {
                        bestMatch = i;
                        var itemBase = ItemBase.Get(itm.ItemId);
                        if (itemBase != null)
                        {
                            if (itemBase.ItemType == ItemType.Bag)
                            {
                                if (hotbarInstance.BagId == itm.BagId)
                                {
                                    break;
                                }
                            }
                            else if (itemBase.ItemType == ItemType.Equipment)
                            {
                                if (hotbarInstance.PreferredStatBuffs != null)
                                {
                                    var statMatch = true;
                                    for (var s = 0; s < hotbarInstance.PreferredStatBuffs.Length; s++)
                                    {
                                        if (itm.ItemProperties.StatModifiers[s] != hotbarInstance.PreferredStatBuffs[s])
                                        {
                                            statMatch = false;
                                        }
                                    }

                                    if (statMatch)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return bestMatch;
        }

        public bool IsEquipped(int slot)
        {
            for (var i = 0; i < Options.EquipmentSlots.Count; i++)
            {
                if (MyEquipment[i] == slot)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsItemOnCooldown(int slot)
        {
            if (Inventory[slot] != null)
            {
                var itm = Inventory[slot];
                if (itm.ItemId != Guid.Empty)
                {
                    if (ItemCooldowns.ContainsKey(itm.ItemId) && ItemCooldowns[itm.ItemId] > Timing.Global.Milliseconds)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public long GetItemRemainingCooldown(int slot)
        {
            if (Inventory[slot] != null)
            {
                var itm = Inventory[slot];
                if (itm.ItemId != Guid.Empty)
                {
                    if (ItemCooldowns.ContainsKey(itm.ItemId) && ItemCooldowns[itm.ItemId] > Timing.Global.Milliseconds)
                    {
                        return ItemCooldowns[itm.ItemId] - Timing.Global.Milliseconds;
                    }
                }
            }

            return 0;
        }

        public bool IsSpellOnCooldown(int slot)
        {
            if (Spells[slot] != null)
            {
                var spl = Spells[slot];
                if (spl.Id != Guid.Empty)
                {
                    if (SpellCooldowns.ContainsKey(spl.Id) && SpellCooldowns[spl.Id] > Timing.Global.Milliseconds)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public long GetSpellRemainingCooldown(int slot)
        {
            if (Spells[slot] != null)
            {
                var spl = Spells[slot];
                if (spl.Id != Guid.Empty)
                {
                    if (SpellCooldowns.ContainsKey(spl.Id) && SpellCooldowns[spl.Id] > Timing.Global.Milliseconds)
                    {
                        return ItemCooldowns[spl.Id] - Timing.Global.Milliseconds;
                    }
                }
            }

            return 0;
        }

        public void TrySellItem(int inventorySlotIndex)
        {
            var inventorySlot = Inventory[inventorySlotIndex];
            if (!ItemBase.TryGet(inventorySlot.ItemId, out var itemDescriptor))
            {
                return;
            }

            var shop = Globals.GameShop;
            var shopCanBuyItem =
                shop.BuyingWhitelist && shop.BuyingItems.Any(buyingItem => buyingItem.ItemId == itemDescriptor.Id)
                || !shop.BuyingWhitelist && !shop.BuyingItems.Any(buyingItem => buyingItem.ItemId == itemDescriptor.Id);

            var prompt = Strings.Shop.sellprompt;
            var inputType = InputBox.InputType.YesNo;
            EventHandler onSuccess = SellInputBoxOkay;
            var userData = inventorySlotIndex;
            var slotQuantity = inventorySlot.Quantity;
            var maxQuantity = slotQuantity;

            if (!shopCanBuyItem)
            {
                prompt = Strings.Shop.cannotsell;
                inputType = InputBox.InputType.OkayOnly;
                onSuccess = null;
                userData = -1;
            }
            else if (itemDescriptor.IsStackable || slotQuantity > 1)
            {
                var inventoryQuantity = GetQuantityOfItemInInventory(itemDescriptor.Id);
                if (inventoryQuantity > 1)
                {
                    maxQuantity = inventoryQuantity;
                    prompt = Strings.Shop.sellitemprompt;
                    inputType = InputBox.InputType.NumericSliderInput;
                    onSuccess = SellItemInputBoxOkay;
                    userData = inventorySlotIndex;
                }
            }

            InputBox.Open(
                title: Strings.Shop.sellitem,
                prompt: prompt.ToString(itemDescriptor.Name),
                modal: true,
                inputType: inputType,
                onSuccess: onSuccess,
                onCancel: null,
                userData: userData,
                quantity: slotQuantity,
                maxQuantity: maxQuantity
            );
        }

        public void TryBuyItem(int shopSlotIndex)
        {
            //Confirm the purchase
            var shopSlot = Globals.GameShop.SellingItems[shopSlotIndex];
            if (!ItemBase.TryGet(shopSlot.ItemId, out var itemDescriptor))
            {
                return;
            }

            var maxBuyAmount = 0;
            if (shopSlot.CostItemQuantity < 1)
            {
                var emptySlots = Inventory.Count(inventorySlot => inventorySlot.ItemId == default);
                var slotsWithItem = Inventory.Count(inventorySlot => inventorySlot.ItemId == itemDescriptor.Id);
                var currentItemQuantity = GetQuantityOfItemInInventory(shopSlot.ItemId);
                var stackSize = itemDescriptor.IsStackable ? itemDescriptor.MaxInventoryStack : 1;
                var itemQuantityLimitInCurrentStacks = slotsWithItem * stackSize;
                var partialStackEmptyQuantity = itemQuantityLimitInCurrentStacks - currentItemQuantity;
                maxBuyAmount = emptySlots * stackSize + partialStackEmptyQuantity;
            }
            else
            {
                var currencyCount = GetQuantityOfItemInInventory(shopSlot.CostItemId);
                maxBuyAmount = (int)Math.Floor(currencyCount / (float)shopSlot.CostItemQuantity);
            }

            // If our max buy amount is 0 or the item isn't stackable, let the server handle it
            if (!itemDescriptor.IsStackable || maxBuyAmount == 0)
            {
                PacketSender.SendBuyItem(shopSlotIndex, 1);
                return;
            }

            InputBox.Open(
                title: Strings.Shop.buyitem,
                prompt: Strings.Shop.buyitemprompt.ToString(itemDescriptor.Name),
                modal: true,
                inputType: InputBox.InputType.NumericSliderInput,
                onSuccess: BuyItemInputBoxOkay,
                onCancel: null,
                userData: shopSlotIndex,
                quantity: maxBuyAmount,
                maxQuantity: maxBuyAmount
            );
        }

        private void BuyItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                PacketSender.SendBuyItem((int)((InputBox)sender).UserData, value);
            }
        }

        private void SellItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                PacketSender.SendSellItem((int)((InputBox)sender).UserData, value);
            }
        }

        private void SellInputBoxOkay(object sender, EventArgs e)
        {
            PacketSender.SendSellItem((int)((InputBox)sender).UserData, 1);
        }

        //bank
        public void TryDepositItem(int inventorySlotIndex, int bankSlotIndex = -1)
        {
            var inventorySlot = Inventory[inventorySlotIndex];
            if (!ItemBase.TryGet(inventorySlot.ItemId, out var itemDescriptor))
            {
                return;
            }

            //Permission Check
            if (Globals.GuildBank)
            {
                var rank = Globals.Me.GuildRank;
                if (string.IsNullOrWhiteSpace(Globals.Me.Guild) || (!rank.Permissions.BankDeposit && Globals.Me.Rank != 0))
                {
                    ChatboxMsg.AddMessage(new ChatboxMsg(Strings.Guilds.NotAllowedDeposit.ToString(Globals.Me.Guild), CustomColors.Alerts.Error, ChatMessageType.Bank));
                    return;
                }
            }

            var inventoryQuantity = GetQuantityOfItemInInventory(itemDescriptor.Id);
            if (inventorySlot.Quantity < 2)
            {
                PacketSender.SendDepositItem(inventorySlotIndex, 1, bankSlotIndex);
            }

            var userData = new int[2] { inventorySlotIndex, bankSlotIndex };

            InputBox.Open(
                title: Strings.Bank.deposititem,
                prompt: Strings.Bank.deposititemprompt.ToString(itemDescriptor.Name),
                modal: true,
                inputType: InputBox.InputType.NumericSliderInput,
                onSuccess: DepositItemInputBoxOkay,
                onCancel: null,
                userData: userData,
                quantity: inventorySlot.Quantity,
                maxQuantity: inventoryQuantity
            );

        }

        private void DepositItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                var userData = (int[])((InputBox)sender).UserData;

                PacketSender.SendDepositItem(userData[0], value, userData[1]);
            }
        }

        public void TryWithdrawItem(int bankSlotIndex, int inventorySlotIndex = -1)
        {
            var bankSlot = Globals.Bank[bankSlotIndex];
            if (bankSlot == default)
            {
                return;
            }

            if (!ItemBase.TryGet(bankSlot.ItemId, out var itemDescriptor))
            {
                return;
            }

            //Permission Check
            if (Globals.GuildBank)
            {
                var rank = Globals.Me.GuildRank;
                if (string.IsNullOrWhiteSpace(Globals.Me.Guild) || (!rank.Permissions.BankRetrieve && Globals.Me.Rank != 0))
                {
                    ChatboxMsg.AddMessage(new ChatboxMsg(Strings.Guilds.NotAllowedWithdraw.ToString(Globals.Me.Guild), CustomColors.Alerts.Error, ChatMessageType.Bank));
                    return;
                }
            }

            var quantity = bankSlot.Quantity;
            var maxQuantity = quantity;

            if (itemDescriptor.IsStackable)
            {
                maxQuantity = GetQuantityOfItemInBank(itemDescriptor.Id);
            }

            if (maxQuantity < 2)
            {
                PacketSender.SendWithdrawItem(bankSlotIndex, 1, inventorySlotIndex);
                return;
            }

            int[] userData = new int[2] { bankSlotIndex, inventorySlotIndex };

            InputBox.Open(
                title: Strings.Bank.withdrawitem,
                prompt: Strings.Bank.withdrawitemprompt.ToString(itemDescriptor.Name),
                modal: true,
                inputType: InputBox.InputType.NumericSliderInput,
                onSuccess: WithdrawItemInputBoxOkay,
                onCancel: null,
                userData: userData,
                quantity: quantity,
                maxQuantity: maxQuantity
            );
        }

        private void WithdrawItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                int[] userData = (int[])((InputBox)sender).UserData;
                PacketSender.SendWithdrawItem(userData[0], value, userData[1]);
            }
        }

        //Bag
        public void TryStoreBagItem(int inventorySlotIndex, int bagSlotIndex)
        {
            var inventorySlot = Inventory[inventorySlotIndex];
            if (!ItemBase.TryGet(inventorySlot.ItemId, out var itemDescriptor))
            {
                return;
            }

            int[] userData = new int[2] { inventorySlotIndex, bagSlotIndex };

            var quantity = inventorySlot.Quantity;
            var maxQuantity = quantity;

            // TODO: Refactor server bagging logic to allow transferring quantities from the entire inventory and not just the slot
            //if (itemDescriptor.IsStackable)
            //{
            //    maxQuantity = GetQuantityOfItemInInventory(itemDescriptor.Id);
            //}

            if (maxQuantity < 2)
            {
                PacketSender.SendStoreBagItem(inventorySlotIndex, 1, bagSlotIndex);
                return;
            }

            InputBox.Open(
                title: Strings.Bags.storeitem,
                prompt: Strings.Bags.storeitemprompt.ToString(itemDescriptor.Name), true,
                inputType: InputBox.InputType.NumericSliderInput,
                onSuccess: StoreBagItemInputBoxOkay,
                onCancel: null,
                userData: userData,
                quantity: quantity,
                maxQuantity: maxQuantity
            );
        }

        private void StoreBagItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                int[] userData = (int[])((InputBox)sender).UserData;
                PacketSender.SendStoreBagItem(userData[0], value, userData[1]);
            }
        }

        public void TryRetreiveBagItem(int bagSlotIndex, int inventorySlotIndex)
        {
            var bagSlot = Globals.Bag[bagSlotIndex];
            if (bagSlot == default)
            {
                return;
            }

            if (!ItemBase.TryGet(bagSlot.ItemId, out var itemDescriptor))
            {
                return;
            }

            var userData = new int[2] { bagSlotIndex, inventorySlotIndex };

            var quantity = bagSlot.Quantity;
            var maxQuantity = quantity;

            // TODO: Refactor server bagging logic to allow transferring quantities from the entire inventory and not just the slot
            //if (itemDescriptor.IsStackable)
            //{
            //    maxQuantity = GetQuantityOfItemInBag(itemDescriptor.Id);
            //}

            if (maxQuantity < 2)
            {
                PacketSender.SendRetrieveBagItem(bagSlotIndex, 1, inventorySlotIndex);
                return;
            }

            InputBox.Open(
                title: Strings.Bags.retreiveitem,
                prompt: Strings.Bags.retreiveitemprompt.ToString(itemDescriptor.Name),
                modal: true,
                inputType: InputBox.InputType.NumericSliderInput,
                onSuccess: RetreiveBagItemInputBoxOkay,
                onCancel: null,
                userData: userData,
                quantity: quantity,
                maxQuantity: maxQuantity
            );
        }

        private void RetreiveBagItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                int[] userData = (int[])((InputBox)sender).UserData;
                PacketSender.SendRetrieveBagItem(userData[0], value, userData[1]);
            }
        }

        //Trade
        public void TryTradeItem(int index)
        {
            var slot = Inventory[index];
            var quantity = slot.Quantity;
            var tradingItem = ItemBase.Get(slot.ItemId);
            if (tradingItem == null)
            {
                return;
            }

            if (quantity == 1)
            {
                PacketSender.SendOfferTradeItem(index, 1);
                return;
            }

            InputBox.Open(
                title: Strings.Trading.offeritem,
                prompt: Strings.Trading.offeritemprompt.ToString(tradingItem.Name),
                modal: true,
                inputType: InputBox.InputType.NumericSliderInput,
                onSuccess: TradeItemInputBoxOkay,
                onCancel: null,
                userData: index,
                quantity: quantity,
                maxQuantity: quantity
            );
        }

        private void TradeItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                PacketSender.SendOfferTradeItem((int)((InputBox)sender).UserData, value);
            }
        }

        public void TryRevokeItem(int index)
        {
            var slot = Globals.Trade[0, index];
            var quantity = slot.Quantity;
            var revokedItem = ItemBase.Get(slot.ItemId);
            if (revokedItem == null)
            {
                return;
            }

            if (quantity == 1)
            {
                PacketSender.SendRevokeTradeItem(index, 1);
                return;
            }

            InputBox.Open(
                title: Strings.Trading.revokeitem,
                prompt: Strings.Trading.revokeitemprompt.ToString(revokedItem.Name),
                modal: true,
                inputType: InputBox.InputType.NumericSliderInput,
                onSuccess: RevokeItemInputBoxOkay,
                onCancel: null,
                userData: index,
                quantity: quantity,
                maxQuantity: quantity
            );
        }

        private void RevokeItemInputBoxOkay(object sender, EventArgs e)
        {
            var value = (int)Math.Round(((InputBox)sender).Value);
            if (value > 0)
            {
                PacketSender.SendRevokeTradeItem((int)((InputBox)sender).UserData, value);
            }
        }

        //Spell Processing
        public void SwapSpells(int fromSpellIndex, int toSpellIndex)
        {
            PacketSender.SendSwapSpells(fromSpellIndex, toSpellIndex);
            var tmpInstance = Spells[toSpellIndex];
            Spells[toSpellIndex] = Spells[fromSpellIndex];
            Spells[fromSpellIndex] = tmpInstance;
        }

        public void TryForgetSpell(int spellIndex)
        {
            var spellSlot = Spells[spellIndex];
            if (SpellBase.TryGet(spellSlot.Id, out var spellDescriptor))
            {
                InputBox.Open(
                    title: Strings.Spells.forgetspell,
                    prompt: Strings.Spells.forgetspellprompt.ToString(spellDescriptor.Name),
                    modal: true,
                    inputType: InputBox.InputType.YesNo,
                    onSuccess: ForgetSpellInputBoxOkay,
                    onCancel: null,
                    userData: spellIndex
                );
            }
        }

        private void ForgetSpellInputBoxOkay(object sender, EventArgs e)
        {
            PacketSender.SendForgetSpell((int)((InputBox)sender).UserData);
        }

        public void TryUseSpell(int index)
        {
            if (Spells[index].Id != Guid.Empty &&
                (!Globals.Me.SpellCooldowns.ContainsKey(Spells[index].Id) ||
                 Globals.Me.SpellCooldowns[Spells[index].Id] < Timing.Global.Milliseconds))
            {
                var spellBase = SpellBase.Get(Spells[index].Id);

                if (spellBase.CastDuration > 0 && (Options.Instance.CombatOpts.MovementCancelsCast && Globals.Me.IsMoving))
                {
                    return;
                }

                PacketSender.SendUseSpell(index, TargetIndex);
            }
        }

        public long GetSpellCooldown(Guid id)
        {
            if (SpellCooldowns.ContainsKey(id))
            {
                return SpellCooldowns[id];
            }

            return 0;
        }

        public void TryUseSpell(Guid spellId)
        {
            if (spellId == Guid.Empty)
            {
                return;
            }

            for (var i = 0; i < Spells.Length; i++)
            {
                if (Spells[i].Id == spellId)
                {
                    TryUseSpell(i);

                    return;
                }
            }
        }

        public int FindHotbarSpell(IHotbarInstance hotbarInstance)
        {
            if (hotbarInstance.ItemOrSpellId != Guid.Empty && SpellBase.Get(hotbarInstance.ItemOrSpellId) != null)
            {
                for (var i = 0; i < Spells.Length; i++)
                {
                    if (Spells[i].Id == hotbarInstance.ItemOrSpellId)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        //Hotbar Processing
        public void AddToHotbar(int hotbarSlot, sbyte itemType, int itemSlot)
        {
            Hotbar[hotbarSlot].ItemOrSpellId = Guid.Empty;
            Hotbar[hotbarSlot].PreferredStatBuffs = new int[(int)Enums.Stat.StatCount];
            if (itemType == 0)
            {
                var item = Inventory[itemSlot];
                if (item != null)
                {
                    Hotbar[hotbarSlot].ItemOrSpellId = item.ItemId;
                    Hotbar[hotbarSlot].PreferredStatBuffs = item.ItemProperties.StatModifiers;
                }
            }
            else if (itemType == 1)
            {
                var spell = Spells[itemSlot];
                if (spell != null)
                {
                    Hotbar[hotbarSlot].ItemOrSpellId = spell.Id;
                }
            }

            PacketSender.SendHotbarUpdate(hotbarSlot, itemType, itemSlot);
        }

        public void HotbarSwap(int index, int swapIndex)
        {
            var itemId = Hotbar[index].ItemOrSpellId;
            var bagId = Hotbar[index].BagId;
            var stats = Hotbar[index].PreferredStatBuffs;

            Hotbar[index].ItemOrSpellId = Hotbar[swapIndex].ItemOrSpellId;
            Hotbar[index].BagId = Hotbar[swapIndex].BagId;
            Hotbar[index].PreferredStatBuffs = Hotbar[swapIndex].PreferredStatBuffs;

            Hotbar[swapIndex].ItemOrSpellId = itemId;
            Hotbar[swapIndex].BagId = bagId;
            Hotbar[swapIndex].PreferredStatBuffs = stats;

            PacketSender.SendHotbarSwap(index, swapIndex);
        }

        // Change the dimension if the player is on a gateway
        private void TryToChangeDimension()
        {
            if (X < Options.MapWidth && X >= 0)
            {
                if (Y < Options.MapHeight && Y >= 0)
                {
                    if (Maps.MapInstance.Get(MapId) != null && Maps.MapInstance.Get(MapId).Attributes[X, Y] != null)
                    {
                        if (Maps.MapInstance.Get(MapId).Attributes[X, Y].Type == MapAttribute.ZDimension)
                        {
                            if (((MapZDimensionAttribute)Maps.MapInstance.Get(MapId).Attributes[X, Y]).GatewayTo > 0)
                            {
                                Z = (byte)(((MapZDimensionAttribute)Maps.MapInstance.Get(MapId).Attributes[X, Y])
                                            .GatewayTo -
                                            1);
                            }
                        }
                    }
                }
            }
        }

        //Input Handling
        private void HandleInput()
        {
            var movex = 0f;
            var movey = 0f;
            if (Interface.Interface.HasInputFocus())
            {
                return;
            }

            if (Controls.KeyDown(Control.MoveUp))
            {
                movey = 1;
            }

            if (Controls.KeyDown(Control.MoveDown))
            {
                movey = -1;
            }

            if (Controls.KeyDown(Control.MoveLeft))
            {
                movex = -1;
            }

            if (Controls.KeyDown(Control.MoveRight))
            {
                movex = 1;
            }

            Globals.Me.MoveDir = Direction.None;
            if (movex != 0f || movey != 0f)
            {
                var diagonalMovement = movex != 0 && Options.Instance.MapOpts.EnableDiagonalMovement;
                if (movey < 0)
                {
                    if (diagonalMovement)
                    {
                        Globals.Me.MoveDir = movex < 0 ? Direction.DownLeft : Direction.DownRight;
                    }
                    else
                    {
                        Globals.Me.MoveDir = Direction.Down;
                    }
                }
                else if (movey > 0)
                {
                    if (diagonalMovement)
                    {
                        Globals.Me.MoveDir = movex < 0 ? Direction.UpLeft : Direction.UpRight;
                    }
                    else
                    {
                        Globals.Me.MoveDir = Direction.Up;
                    }
                }
                else
                {
                    Globals.Me.MoveDir = movex < 0 ? Direction.Left : Direction.Right;
                }
            }
            
            TurnAround();

            var castInput = -1;
            for (var barSlot = 0; barSlot < Options.Instance.PlayerOpts.HotbarSlotCount; barSlot++)
            {
                if (!mLastHotbarUseTime.ContainsKey(barSlot))
                {
                    mLastHotbarUseTime.Add(barSlot, 0);
                }

                if (Controls.KeyDown((Control)barSlot + (int)Control.Hotkey1))
                {
                    castInput = barSlot;
                }
            }

            if (castInput != -1)
            {
                if (0 <= castInput && castInput < Interface.Interface.GameUi?.Hotbar?.Items?.Count && mLastHotbarUseTime[castInput] < Timing.Global.Milliseconds)
                {
                    Interface.Interface.GameUi?.Hotbar?.Items?[castInput]?.Activate();
                    mLastHotbarUseTime[castInput] = Timing.Global.Milliseconds + mHotbarUseDelay;
                }
            }
        }

        protected int GetDistanceTo(IEntity target)
        {
            if (target != null)
            {
                var myMap = Maps.MapInstance.Get(MapId);
                var targetMap = Maps.MapInstance.Get(target.MapId);
                if (myMap != null && targetMap != null)
                {
                    //Calculate World Tile of Me
                    var x1 = X + myMap.GridX * Options.MapWidth;
                    var y1 = Y + myMap.GridY * Options.MapHeight;

                    //Calculate world tile of target
                    var x2 = target.X + targetMap.GridX * Options.MapWidth;
                    var y2 = target.Y + targetMap.GridY * Options.MapHeight;

                    return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
                }
            }

            //Something is null.. return a value that is out of range :)
            return 9999;
        }

        public void AutoTarget()
        {
            //Check for taunt status if so don't allow to change target
            for (var i = 0; i < Status.Count; i++)
            {
                if (Status[i].Type == SpellEffect.Taunt)
                {
                    return;
                }
            }

            // Do we need to account for players?
            // Depends on what type of map we're currently on.
            if (Globals.Me.MapInstance == null)
            {
                return;
            }
            var currentMap = Globals.Me.MapInstance as MapInstance;
            var canTargetPlayers = currentMap.ZoneType != MapZone.Safe;

            // Build a list of Entities to select from with positions if our list is either old, we've moved or changed maps somehow.
            if (
                mlastTargetScanTime < Timing.Global.Milliseconds ||
                mlastTargetScanMap != Globals.Me.MapId ||
                mlastTargetScanLocation != new Point(X, Y)
                )
            {
                // Add new items to our list!
                foreach (var en in Globals.Entities)
                {
                    // Check if this is a valid entity.
                    if (en.Value == null)
                    {
                        continue;
                    }

                    // Don't allow us to auto target ourselves.
                    if (en.Value == Globals.Me)
                    {
                        continue;
                    }

                    // Check if the entity has stealth status
                    if (en.Value.IsHidden || (en.Value.IsStealthed && !Globals.Me.IsInMyParty(en.Value.Id)))
                    {
                        continue;
                    }

                    // Check if we are allowed to target players here, if we're not and this is a player then skip!
                    // If we are, check to see if they're our party or nation member, then exclude them. We're friendly happy people here.
                    if (!canTargetPlayers && en.Value.Type == EntityType.Player)
                    {
                        continue;
                    }
                    else if (canTargetPlayers && en.Value.Type == EntityType.Player)
                    {
                        var player = en.Value as Player;
                        if (IsInMyParty(player))
                        {
                            continue;
                        }
                    }

                    if (en.Value.Type == EntityType.GlobalEntity || en.Value.Type == EntityType.Player)
                    {
                        // Already in our list?
                        if (mlastTargetList.ContainsKey(en.Value))
                        {
                            mlastTargetList[en.Value].DistanceTo = GetDistanceTo(en.Value);
                        }
                        else
                        {
                            // Add entity with blank time. Never been selected.
                            mlastTargetList.Add(en.Value, new TargetInfo() { DistanceTo = GetDistanceTo(en.Value), LastTimeSelected = 0 });
                        }
                    }
                }

                // Remove old items.
                var toRemove = mlastTargetList.Where(en => !Globals.Entities.ContainsValue(en.Key)).ToArray();
                foreach (var en in toRemove)
                {
                    mlastTargetList.Remove(en.Key);
                }

                // Skip scanning for another second or so.. And set up other values.
                mlastTargetScanTime = Timing.Global.Milliseconds + 300;
                mlastTargetScanMap = MapId;
                mlastTargetScanLocation = new Point(X, Y);
            }

            // Find valid entities.
            var validEntities = mlastTargetList.ToArray();

            // Reduce the number of targets down to what is in our allowed range.
            validEntities = validEntities.Where(en => en.Value.DistanceTo <= Options.Combat.MaxPlayerAutoTargetRadius).ToArray();

            int currentDistance = 9999;
            long currentTime = Timing.Global.Milliseconds;
            Entity currentEntity = mLastEntitySelected;
            foreach (var entity in validEntities)
            {
                if (currentEntity == entity.Key)
                {
                    continue;
                }

                // if distance is the same
                if (entity.Value.DistanceTo == currentDistance)
                {
                    if (entity.Value.LastTimeSelected < currentTime)
                    {
                        currentTime = entity.Value.LastTimeSelected;
                        currentDistance = entity.Value.DistanceTo;
                        currentEntity = entity.Key;
                    }
                }
                else if (entity.Value.DistanceTo < currentDistance)
                {
                    if (entity.Value.LastTimeSelected < currentTime || entity.Value.LastTimeSelected == currentTime)
                    {
                        currentTime = entity.Value.LastTimeSelected;
                        currentDistance = entity.Value.DistanceTo;
                        currentEntity = entity.Key;
                    }
                }
            }

            // We didn't target anything? Can we default to closest?
            if (currentEntity == null)
            {
                currentEntity = validEntities.Where(x => x.Value.DistanceTo == validEntities.Min(y => y.Value.DistanceTo)).FirstOrDefault().Key;

                // Also reset our target times so we can start auto targetting again.
                foreach (var entity in mlastTargetList)
                {
                    entity.Value.LastTimeSelected = 0;
                }
            }

            if (currentEntity == null)
            {
                mLastEntitySelected = null;
                return;
            }

            if (mlastTargetList.ContainsKey(currentEntity))
            {
                mlastTargetList[currentEntity].LastTimeSelected = Timing.Global.Milliseconds;
            }
            mLastEntitySelected = currentEntity as Entity;

            if (TargetIndex != currentEntity.Id)
            {
                SetTargetBox(currentEntity as Entity);
                TargetIndex = currentEntity.Id;
                TargetType = 0;
            }
        }

        private void SetTargetBox(Entity en)
        {
            if (en == null)
            {
                TargetBox?.SetEntity(null);
                TargetBox?.Hide();
                return;
            }

            if (en is Player)
            {
                TargetBox?.SetEntity(en, EntityType.Player);
            }
            else if (en is Event)
            {
                TargetBox?.SetEntity(en, EntityType.Event);
            }
            else
            {
                TargetBox?.SetEntity(en, EntityType.GlobalEntity);
            }

            TargetBox?.Show();
        }

        private void AutoTurnToTarget(Entity en)
        {
            if (en == this)
            {
                return;
            }

            if (!Globals.Database.AutoTurnToTarget)
            {
                return;
            }

            if (!Options.Instance.PlayerOpts.EnableAutoTurnToTarget)
            {
                return;
            }

            if (Controls.KeyDown(Control.TurnAround))
            {
                return;
            }

            var directionToTarget = DirectionToTarget(en);

            if (IsMoving || Dir == MoveDir || Dir == directionToTarget)
            {
                AutoTurnToTargetTimer = Timing.Global.Milliseconds + Options.Instance.PlayerOpts.AutoTurnToTargetDelay;
                return;
            }

            if (AutoTurnToTargetTimer > Timing.Global.Milliseconds)
            {
                return;
            }

            if (Options.Instance.PlayerOpts.AutoTurnToTargetIgnoresEntitiesBehind &&
                IsTargetAtOppositeDirection(Dir, directionToTarget))
            {
                return;
            }

            MoveDir = Direction.None;
            Dir = directionToTarget;
            PacketSender.SendDirection(Dir);
            PickLastDirection(Dir);
        }

        public bool TryBlock()
        {
            var shieldIndex = Options.ShieldIndex;
            var myShieldIndex = MyEquipment[shieldIndex];

            // Return false if character is attacking or if they don't have a shield equipped.
            if (IsAttacking || shieldIndex < 0 || myShieldIndex < 0)
            {
                return false;
            }

            // Return false if the shield item descriptor could not be retrieved.
            if (!ItemBase.TryGet(Inventory[myShieldIndex].ItemId, out _))
            {
                return false;
            }

            IsBlocking = true;
            PacketSender.SendBlock(IsBlocking);
            return IsBlocking;
        }

        public bool TryAttack()
        {
            if (IsAttacking || IsBlocking || (IsMoving && !Options.Instance.PlayerOpts.AllowCombatMovement))
            {
                return false;
            }

            int x = Globals.Me.X;
            int y = Globals.Me.Y;
            var map = Globals.Me.MapId;

            switch (Globals.Me.Dir)
            {
                case Direction.Up:
                    y--;
                    break;

                case Direction.Down:
                    y++;
                    break;

                case Direction.Left:
                    x--;
                    break;

                case Direction.Right:
                    x++;
                    break;

                case Direction.UpLeft:
                    y--;
                    x--;
                    break;

                case Direction.UpRight:
                    y--;
                    x++;
                    break;

                case Direction.DownRight:
                    y++;
                    x++;
                    break;

                case Direction.DownLeft:
                    y++;
                    x--;
                    break;
            }

            if (TryGetRealLocation(ref x, ref y, ref map))
            {
                // Iterate through all entities
                foreach (var en in Globals.Entities)
                {
                    // Skip if the entity is null or is not within the player's map.
                    if (en.Value?.MapId != map)
                    {
                        continue;
                    }

                    // Skip if the entity is the current player.
                    if (en.Value == Globals.Me)
                    {
                        continue;
                    }

                    // Skip if the entity can't be attacked.
                    if (!en.Value.CanBeAttacked)
                    {
                        continue;
                    }

                    if (en.Value.X != x || en.Value.Y != y)
                    {
                        continue;
                    }

                    // Attack the entity.
                    PacketSender.SendAttack(en.Key);
                    AttackTimer = Timing.Global.Milliseconds + CalculateAttackTime();

                    return true;
                }
            }

            foreach (MapInstance eventMap in Maps.MapInstance.Lookup.Values)
            {
                foreach (var en in eventMap.LocalEntities)
                {
                    if (en.Value == null)
                    {
                        continue;
                    }

                    if (en.Value.MapId == map && en.Value.X == x && en.Value.Y == y)
                    {
                        if (en.Value is Event)
                        {
                            //Talk to Event
                            PacketSender.SendActivateEvent(en.Key);
                            AttackTimer = Timing.Global.Milliseconds + CalculateAttackTime();

                            return true;
                        }
                    }
                }
            }

            //Projectile/empty swing for animations
            PacketSender.SendAttack(Guid.Empty);
            AttackTimer = Timing.Global.Milliseconds + CalculateAttackTime();

            return true;
        }

        public bool TryGetRealLocation(ref int x, ref int y, ref Guid mapId)
        {
            var tmpX = x;
            var tmpY = y;
            var tmpI = -1;
            if (Maps.MapInstance.Get(mapId) != null)
            {
                var gridX = Maps.MapInstance.Get(mapId).GridX;
                var gridY = Maps.MapInstance.Get(mapId).GridY;

                if (x < 0)
                {
                    tmpX = Options.MapWidth - x * -1;
                    gridX--;
                }

                if (y < 0)
                {
                    tmpY = Options.MapHeight - y * -1;
                    gridY--;
                }

                if (y > Options.MapHeight - 1)
                {
                    tmpY = y - Options.MapHeight;
                    gridY++;
                }

                if (x > Options.MapWidth - 1)
                {
                    tmpX = x - Options.MapWidth;
                    gridX++;
                }

                if (gridX >= 0 && gridX < Globals.MapGridWidth && gridY >= 0 && gridY < Globals.MapGridHeight)
                {
                    if (Maps.MapInstance.Get(Globals.MapGrid[gridX, gridY]) != null)
                    {
                        x = (byte)tmpX;
                        y = (byte)tmpY;
                        mapId = Globals.MapGrid[gridX, gridY];

                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryTarget()
        {
            //Check for taunt status if so don't allow to change target
            for (var i = 0; i < Status.Count; i++)
            {
                if (Status[i].Type == SpellEffect.Taunt)
                {
                    return false;
                }
            }

            var x = (int)Math.Floor(Globals.InputManager.GetMousePosition().X + Graphics.CurrentView.Left);
            var y = (int)Math.Floor(Globals.InputManager.GetMousePosition().Y + Graphics.CurrentView.Top);
            var targetRect = new FloatRect(x - 8, y - 8, 16, 16); //Adjust to allow more/less error

            IEntity bestMatch = null;
            var bestAreaMatch = 0f;


            foreach (MapInstance map in Maps.MapInstance.Lookup.Values)
            {
                if (x >= map.GetX() && x <= map.GetX() + Options.MapWidth * Options.TileWidth)
                {
                    if (y >= map.GetY() && y <= map.GetY() + Options.MapHeight * Options.TileHeight)
                    {
                        //Remove the offsets to just be dealing with pixels within the map selected
                        x -= (int)map.GetX();
                        y -= (int)map.GetY();

                        //transform pixel format to tile format
                        x /= Options.TileWidth;
                        y /= Options.TileHeight;
                        var mapId = map.Id;

                        if (TryGetRealLocation(ref x, ref y, ref mapId))
                        {
                            foreach (var en in Globals.Entities)
                            {
                                if (en.Value == null || en.Value.MapId != mapId || en.Value is Projectile || en.Value is Resource || (en.Value.IsStealthed && !Globals.Me.IsInMyParty(en.Value.Id)))
                                {
                                    continue;
                                }

                                var intersectRect = FloatRect.Intersect(en.Value.WorldPos, targetRect);
                                if (intersectRect.Width * intersectRect.Height > bestAreaMatch)
                                {
                                    bestAreaMatch = intersectRect.Width * intersectRect.Height;
                                    bestMatch = en.Value;
                                }
                            }

                            foreach (MapInstance eventMap in Maps.MapInstance.Lookup.Values)
                            {
                                foreach (var en in eventMap.LocalEntities)
                                {
                                    if (en.Value == null || en.Value.MapId != mapId || ((Event)en.Value).DisablePreview)
                                    {
                                        continue;
                                    }

                                    var intersectRect = FloatRect.Intersect(en.Value.WorldPos, targetRect);
                                    if (intersectRect.Width * intersectRect.Height > bestAreaMatch)
                                    {
                                        bestAreaMatch = intersectRect.Width * intersectRect.Height;
                                        bestMatch = en.Value;
                                    }
                                }
                            }

                            if (bestMatch != null && bestMatch.Id != TargetIndex)
                            {
                                var targetType = bestMatch is Event ? 1 : 0;


                                SetTargetBox(bestMatch as Entity);

                                if (bestMatch is Player)
                                {
                                    //Select in admin window if open
                                    if (Interface.Interface.GameUi.AdminWindowOpen())
                                    {
                                        Interface.Interface.GameUi.AdminWindowSelectName(bestMatch.Name);
                                    }
                                }

                                TargetType = targetType;
                                TargetIndex = bestMatch.Id;

                                return true;
                            }
                            else if (!Globals.Database.StickyTarget)
                            {
                                // We've clicked off of our target and are allowed to clear it!
                                ClearTarget();
                                return true;
                            }
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        public bool TryTarget(IEntity entity, bool force = false)
        {
            //Check for taunt status if so don't allow to change target
            for (var i = 0; i < Status.Count; i++)
            {
                if (Status[i].Type == SpellEffect.Taunt && !force)
                {
                    return false;
                }
            }

            if (entity == null)
            {
                return false;
            }

            // Are we already targetting this?
            if (TargetBox != null && TargetBox.MyEntity == entity)
            {
                return true;
            }

            var targetType = entity is Event ? 1 : 0;

            if (entity is Player)
            {
                //Select in admin window if open
                if (Interface.Interface.GameUi.AdminWindowOpen())
                {
                    Interface.Interface.GameUi.AdminWindowSelectName(entity.Name);
                }
            }

            if (TargetIndex != entity.Id)
            {
                SetTargetBox(entity as Entity);
                TargetType = targetType;
                TargetIndex = entity.Id;
            }

            return true;

        }

        public void ClearTarget()
        {
            SetTargetBox(null);

            TargetIndex = Guid.Empty;
            TargetType = -1;
        }

        /// <summary>
        /// Attempts to pick up an item at the specified location.
        /// </summary>
        /// <param name="mapId">The Id of the map we are trying to loot from.</param>
        /// <param name="x">The X location on the current map.</param>
        /// <param name="y">The Y location on the current map.</param>
        /// <param name="uniqueId">The Unique Id of the specific item we want to pick up, leave <see cref="Guid.Empty"/> to not specificy an item and pick up the first thing we can find.</param>
        /// <param name="firstOnly">Defines whether we only want to pick up the first item we can find when true, or all items when false.</param>
        /// <returns></returns>
        public bool TryPickupItem(Guid mapId, int tileIndex, Guid uniqueId = new Guid(), bool firstOnly = false)
        {
            var map = Maps.MapInstance.Get(mapId);
            if (map == null || tileIndex < 0 || tileIndex >= Options.MapWidth * Options.MapHeight)
            {
                return false;
            }

            // Are we trying to pick up anything in particular, or everything?
            if (uniqueId != Guid.Empty || firstOnly)
            {
                if (!map.MapItems.ContainsKey(tileIndex) || map.MapItems[tileIndex].Count < 1)
                {
                    return false;
                }

                foreach (var item in map.MapItems[tileIndex])
                {
                    // Check if we are trying to pick up a specific item, and if this is the one.
                    if (uniqueId != Guid.Empty && item.Id != uniqueId)
                    {
                        continue;
                    }

                    PacketSender.SendPickupItem(mapId, tileIndex, item.Id);

                    return true;
                }
            }
            else
            {
                // Let the server worry about what we can and can not pick up.
                PacketSender.SendPickupItem(mapId, tileIndex, uniqueId);

                return true;
            }

            return false;
        }

        //Forumlas
        public long GetNextLevelExperience()
        {
            return ExperienceToNextLevel;
        }

        public override int CalculateAttackTime()
        {
            ItemBase weapon = null;
            var attackTime = base.CalculateAttackTime();

            var cls = ClassBase.Get(Class);
            if (cls != null && cls.AttackSpeedModifier == 1) //Static
            {
                attackTime = cls.AttackSpeedValue;
            }

            if (this == Globals.Me)
            {
                if (Options.WeaponIndex > -1 &&
                    Options.WeaponIndex < Equipment.Length &&
                    MyEquipment[Options.WeaponIndex] >= 0)
                {
                    weapon = ItemBase.Get(Inventory[MyEquipment[Options.WeaponIndex]].ItemId);
                }
            }
            else
            {
                if (Options.WeaponIndex > -1 &&
                    Options.WeaponIndex < Equipment.Length &&
                    Equipment[Options.WeaponIndex] != Guid.Empty)
                {
                    weapon = ItemBase.Get(Equipment[Options.WeaponIndex]);
                }
            }

            if (weapon != null)
            {
                if (weapon.AttackSpeedModifier == 1) // Static
                {
                    attackTime = weapon.AttackSpeedValue;
                }
                else if (weapon.AttackSpeedModifier == 2) //Percentage
                {
                    attackTime = (int)(attackTime * (100f / weapon.AttackSpeedValue));
                }
            }

            return attackTime;
        }

        /// <summary>
        /// Calculate the attack time for the player as if they have a specified speed stat.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public virtual int CalculateAttackTime(int speed)
        {
            return (int)(Options.MaxAttackRate +
                          (Options.MinAttackRate - Options.MaxAttackRate) *
                          (((float)Options.MaxStatValue - speed) /
                           Options.MaxStatValue));
        }

        //Movement Processing
        private void ProcessDirectionalInput()
        {
            //Check if player is crafting
            if (Globals.InCraft == true)
            {
                return;
            }

            //check if player is stunned or snared, if so don't let them move.
            for (var n = 0; n < Status.Count; n++)
            {
                if (Status[n].Type == SpellEffect.Stun ||
                    Status[n].Type == SpellEffect.Snare ||
                    Status[n].Type == SpellEffect.Sleep)
                {
                    return;
                }
            }

            //Check if the player is dashing, if so don't let them move.
            if (Dashing != null || DashQueue.Count > 0 || DashTimer > Timing.Global.Milliseconds)
            {
                return;
            }

            if (IsAttacking && !Options.Instance.PlayerOpts.AllowCombatMovement)
            {
                return;
            }

            var tmpX = (sbyte)X;
            var tmpY = (sbyte)Y;
            IEntity blockedBy = null;

            if (MoveDir <= Direction.None || Globals.EventDialogs.Count != 0)
            {
                return;
            }

            //Try to move if able and not casting spells.
            if (IsMoving || MoveTimer >= Timing.Global.Milliseconds ||
                (!Options.Combat.MovementCancelsCast && IsCasting))
            {
                return;
            }

            if (Options.Combat.MovementCancelsCast)
            {
                CastTime = 0;
            }

            var deltaX = 0;
            var deltaY = 0;

            switch (MoveDir)
            {
                case Direction.Up:
                    deltaX = 0;
                    deltaY = -1;
                    break;
                
                case Direction.Down:
                    deltaX = 0;
                    deltaY = 1;
                    break;
                
                case Direction.Left:
                    deltaX = -1;
                    deltaY = 0;
                    break;
                
                case Direction.Right:
                    deltaX = 1;
                    deltaY = 0;
                    break;
                
                case Direction.UpLeft:
                    deltaX = -1;
                    deltaY = -1;
                    break;
                
                case Direction.UpRight:
                    deltaX = 1;
                    deltaY = -1;
                    break;
                
                case Direction.DownLeft:
                    deltaX = -1;
                    deltaY = 1;
                    break;
                
                case Direction.DownRight:
                    deltaX = 1;
                    deltaY = 1;
                    break;
            }

            if (deltaX != 0 || deltaY != 0)
            {
                if (IsTileBlocked(tmpX + deltaX, tmpY + deltaY, Z, MapId, ref blockedBy) == -1)
                {
                    tmpX += (sbyte)deltaX;
                    tmpY += (sbyte)deltaY;
                    IsMoving = true;
                    Dir = MoveDir;
                    
                    if (deltaX == 0)
                    {
                        OffsetX = 0;
                    }
                    else
                    {
                        OffsetX = deltaX > 0 ? -Options.TileWidth : Options.TileWidth;
                    }

                    if (deltaY == 0)
                    {
                        OffsetY = 0;
                    }
                    else
                    {
                        OffsetY = deltaY > 0 ? -Options.TileHeight : Options.TileHeight;
                    }
                }
            }

            if (blockedBy != mLastBumpedEvent)
            {
                mLastBumpedEvent = null;
            }

            if (IsMoving)
            {
                if (tmpX < 0 || tmpY < 0 || tmpX > Options.MapWidth - 1 || tmpY > Options.MapHeight - 1)
                {
                    var gridX = Maps.MapInstance.Get(Globals.Me.MapId).GridX;
                    var gridY = Maps.MapInstance.Get(Globals.Me.MapId).GridY;
                    if (tmpX < 0)
                    {
                        gridX--;
                        X = (byte)(Options.MapWidth - 1);
                    }
                    else if (tmpX >= Options.MapWidth)
                    {
                        X = 0;
                        gridX++;
                    }
                    else
                    {
                        X = (byte)tmpX;
                    }

                    if (tmpY < 0)
                    {
                        gridY--;
                        Y = (byte)(Options.MapHeight - 1);
                    }
                    else if (tmpY >= Options.MapHeight)
                    {
                        Y = 0;
                        gridY++;
                    }
                    else
                    {
                        Y = (byte)tmpY;
                    }

                    if (MapId != Globals.MapGrid[gridX, gridY])
                    {
                        MapId = Globals.MapGrid[gridX, gridY];
                        FetchNewMaps();
                    }
                }
                else
                {
                    X = (byte)tmpX;
                    Y = (byte)tmpY;
                }

                TryToChangeDimension();
                PacketSender.SendMove();
                MoveTimer = (Timing.Global.Milliseconds) + (long)GetMovementTime();
            }
            else
            {
                if (MoveDir != Dir)
                {
                    Dir = MoveDir;
                    PacketSender.SendDirection(Dir);
                }

                if (blockedBy != null && mLastBumpedEvent != blockedBy && blockedBy is Event)
                {
                    PacketSender.SendBumpEvent(blockedBy.MapId, blockedBy.Id);
                    mLastBumpedEvent = blockedBy as Entity;
                }
            }
        }

        public void FetchNewMaps()
        {
            if (Globals.MapGridWidth == 0 || Globals.MapGridHeight == 0)
            {
                return;
            }

            if (Maps.MapInstance.Get(Globals.Me.MapId) != null)
            {
                var gridX = Maps.MapInstance.Get(Globals.Me.MapId).GridX;
                var gridY = Maps.MapInstance.Get(Globals.Me.MapId).GridY;
                for (var x = gridX - 1; x <= gridX + 1; x++)
                {
                    for (var y = gridY - 1; y <= gridY + 1; y++)
                    {
                        if (x >= 0 &&
                            x < Globals.MapGridWidth &&
                            y >= 0 &&
                            y < Globals.MapGridHeight &&
                            Globals.MapGrid[x, y] != Guid.Empty)
                        {
                            PacketSender.SendNeedMap(Globals.MapGrid[x, y]);
                        }
                    }
                }
            }
        }

        public override void DrawEquipment(string filename, Color renderColor)
        {
            //check if player is stunned or snared, if so don't let them move.
            for (var n = 0; n < Status.Count; n++)
            {
                if (Status[n].Type == SpellEffect.Transform)
                {
                    return;
                }
            }

            if(HideAllPaperdolls == true)
            {
                return;
            }

            base.DrawEquipment(filename, renderColor);
        }

        //Override of the original function, used for rendering the color of a player based on rank
        public override void DrawName(Color textColor, Color borderColor, Color backgroundColor)
        {
            if (textColor == null)
            {
                switch (AccessLevel)
                {
                    case Access.Moderator:
                        textColor = CustomColors.Names.Players["Moderator"].Name;
                        borderColor = CustomColors.Names.Players["Moderator"].Outline;
                        backgroundColor = CustomColors.Names.Players["Moderator"].Background;
                        break;

                    case Access.Admin:
                        textColor = CustomColors.Names.Players["Admin"].Name;
                        borderColor = CustomColors.Names.Players["Admin"].Outline;
                        backgroundColor = CustomColors.Names.Players["Admin"].Background;
                        break;

                    case Access.None:
                    default:
                        textColor = CustomColors.Names.Players["Normal"].Name;
                        borderColor = CustomColors.Names.Players["Normal"].Outline;
                        backgroundColor = CustomColors.Names.Players["Normal"].Background;
                        break;
                }
            }

            var customColorOverride = NameColor;
            if (customColorOverride != null)
            {
                //We don't want to override the default colors if the color is transparent!
                if (customColorOverride.A != 0)
                {
                    textColor = customColorOverride;
                }
            }

            DrawNameAndLabels(textColor, borderColor, backgroundColor);
        }

        private void DrawNameAndLabels(Color textColor, Color borderColor, Color backgroundColor)
        {
            base.DrawName(textColor, borderColor, backgroundColor);
            DrawLabels(HeaderLabel.Text, 0, HeaderLabel.Color, textColor, borderColor, backgroundColor);
            DrawLabels(FooterLabel.Text, 1, FooterLabel.Color, textColor, borderColor, backgroundColor);
            DrawGuildName(textColor, borderColor, backgroundColor);
        }

        public virtual void DrawGuildName(Color textColor, Color borderColor = null, Color backgroundColor = null)
        {
            if (HideName || Guild == null || Guild.Trim().Length == 0 || !Options.Instance.Guild.ShowGuildNameTagsOverMembers)
            {
                return;
            }

            if (borderColor == null)
            {
                borderColor = Color.Transparent;
            }

            if (backgroundColor == null)
            {
                backgroundColor = Color.Transparent;
            }

            //Check for stealth amoungst status effects.
            for (var n = 0; n < Status.Count; n++)
            {
                //If unit is stealthed, don't render unless the entity is the player.
                if (Status[n].Type == SpellEffect.Stealth)
                {
                    if (this != Globals.Me && !(this is Player player && Globals.Me.IsInMyParty(player)))
                    {
                        return;
                    }
                }
            }

            var map = MapInstance;
            if (map == null)
            {
                return;
            }

            var textSize = Graphics.Renderer.MeasureText(Guild, Graphics.EntityNameFont, 1);

            var x = (int)Math.Ceiling(Origin.X);
            var y = GetLabelLocation(LabelType.Guild);

            if (backgroundColor != Color.Transparent)
            {
                Graphics.DrawGameTexture(
                    Graphics.Renderer.GetWhiteTexture(), new Framework.GenericClasses.FloatRect(0, 0, 1, 1),
                    new Framework.GenericClasses.FloatRect(x - textSize.X / 2f - 4, y, textSize.X + 8, textSize.Y), backgroundColor
                );
            }

            Graphics.Renderer.DrawString(
                Guild, Graphics.EntityNameFont, x - (int)Math.Ceiling(textSize.X / 2f), (int)y, 1,
                Color.FromArgb(textColor.ToArgb()), true, null, Color.FromArgb(borderColor.ToArgb())
            );
        }

        protected override bool ShouldDrawHpBar
        {
            get
            {
                if (ShouldNotDrawHpBar)
                {
                    return false;
                }

                if (IsHovered)
                {
                    return true;
                }

                var me = Globals.Me;

                if (Globals.Database.MyOverheadHpBar && Id == me.Id)
                {
                    return true;
                }

                if (Globals.Database.PlayerOverheadHpBar && Id != me.Id)
                {
                    return true;
                }

                if (Globals.Database.PartyMemberOverheadHpBar && me.IsInMyParty(this))
                {
                    return true;
                }

                if (Globals.Database.FriendOverheadHpBar && me.IsFriend(this))
                {
                    return true;
                }

                return Globals.Database.GuildMemberOverheadHpBar && me.IsGuildMate(this);
            }
        }

        public void DrawTargets()
        {
            foreach (var en in Globals.Entities)
            {
                if (en.Value == null)
                {
                    continue;
                }

                if (en.Value.IsHidden)
                {
                    continue;
                }

                if (en.Value.IsStealthed && (!(en.Value is Player player) || !Globals.Me.IsInMyParty(player)))
                {
                    continue;
                }

                if (en.Value is Projectile || en.Value is Resource)
                {
                    continue;
                }

                if (TargetType != 0 || TargetIndex != en.Value.Id)
                {
                    continue;
                }

                en.Value.DrawTarget((int)Enums.TargetType.Selected);
                AutoTurnToTarget(en.Value);
            }

            foreach (MapInstance eventMap in Maps.MapInstance.Lookup.Values)
            {
                foreach (var en in eventMap.LocalEntities)
                {
                    if (en.Value == null)
                    {
                        continue;
                    }

                    if (en.Value.MapId != eventMap.Id)
                    {
                        continue;
                    }

                    if (en.Value is Event eventEntity && eventEntity.DisablePreview)
                    {
                        continue;
                    }

                    if (en.Value.IsHidden)
                    {
                        continue;
                    }

                    if (en.Value.IsStealthed && (!(en.Value is Player player) || !Globals.Me.IsInMyParty(player)))
                    {
                        continue;
                    }

                    if (TargetType != 1 || TargetIndex != en.Value.Id)
                    {
                        continue;
                    }

                    en.Value.DrawTarget((int)Enums.TargetType.Selected);
                    AutoTurnToTarget(en.Value);
                }
            }

            if (!Interface.Interface.MouseHitGui())
            {
                var mousePos = Graphics.ConvertToWorldPoint(Globals.InputManager.GetMousePosition());
                foreach (MapInstance map in Maps.MapInstance.Lookup.Values)
                {
                    if (mousePos.X >= map.GetX() && mousePos.X <= map.GetX() + Options.MapWidth * Options.TileWidth)
                    {
                        if (mousePos.Y >= map.GetY() &&
                            mousePos.Y <= map.GetY() + Options.MapHeight * Options.TileHeight)
                        {
                            var mapId = map.Id;

                            foreach (var en in Globals.Entities)
                            {
                                if (en.Value == null)
                                {
                                    continue;
                                }

                                if (en.Value.MapId == mapId &&
                                    !en.Value.HideName &&
                                    (!en.Value.IsStealthed ||
                                     en.Value is Player player && Globals.Me.IsInMyParty(player)) &&
                                    en.Value.WorldPos.Contains(mousePos.X, mousePos.Y))
                                {
                                    if (!(en.Value is Projectile || en.Value is Resource))
                                    {
                                        if (TargetType != 0 || TargetIndex != en.Value.Id)
                                        {
                                            en.Value.DrawTarget((int)Enums.TargetType.Hover);
                                        }
                                    }
                                }
                            }

                            foreach (MapInstance eventMap in Maps.MapInstance.Lookup.Values)
                            {
                                foreach (var en in eventMap.LocalEntities)
                                {
                                    if (en.Value == null)
                                    {
                                        continue;
                                    }

                                    if (en.Value.MapId == mapId &&
                                        !(en.Value as Event).DisablePreview &&
                                        !en.Value.IsHidden &&
                                        (!en.Value.IsStealthed ||
                                         en.Value is Player player && Globals.Me.IsInMyParty(player)) &&
                                        en.Value.WorldPos.Contains(mousePos.X, mousePos.Y))
                                    {
                                        if (TargetType != 1 || TargetIndex != en.Value.Id)
                                        {
                                            en.Value.DrawTarget((int)Enums.TargetType.Hover);
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        private partial class TargetInfo
        {
            public long LastTimeSelected;

            public int DistanceTo;
        }

        private void TurnAround()
        {
            // If players hold the 'TurnAround' Control Key and tap to any direction, they will turn on their own axis.
            for (var direction = 0; direction < Options.Instance.MapOpts.MovementDirections; direction++)
            {
                if (!Controls.KeyDown(Control.TurnAround) || direction != (int)Globals.Me.MoveDir)
                {
                    continue;
                }

                // Turn around and hold the player in place if the requested direction is different from the current one.
                if (!Globals.Me.IsMoving && Dir != Globals.Me.MoveDir)
                {
                    Dir = Globals.Me.MoveDir;
                    PacketSender.SendDirection(Dir);
                    Globals.Me.MoveDir = Direction.None;
                    PickLastDirection(Dir);
                }

                // Hold the player in place if the requested direction is the same as the current one.
                if (!Globals.Me.IsMoving && Dir == Globals.Me.MoveDir)
                {
                    Globals.Me.MoveDir = Direction.None;
                }
            }
        }
        
        // Checks if the target is at the opposite direction of the current player's direction.
        // The comparison also takes into account whether diagonal movement is enabled or not.
        private static bool IsTargetAtOppositeDirection(Direction currentDir, Direction targetDir)
        {
            if (Options.Instance.MapOpts.EnableDiagonalMovement)
            {
                // If diagonal movement is disabled, check opposite directions on 4 directions.
                switch (currentDir)
                {
                    case Direction.Up:
                        return targetDir == Direction.Down || targetDir == Direction.DownLeft ||
                               targetDir == Direction.DownRight;

                    case Direction.Down:
                        return targetDir == Direction.Up || targetDir == Direction.UpLeft ||
                               targetDir == Direction.UpRight;

                    case Direction.Left:
                        return targetDir == Direction.Right || targetDir == Direction.UpRight ||
                               targetDir == Direction.DownRight;

                    case Direction.Right:
                        return targetDir == Direction.Left || targetDir == Direction.UpLeft ||
                               targetDir == Direction.DownLeft;

                    default:
                        if (!Options.Instance.MapOpts.EnableDiagonalMovement)
                        {
                            return false;
                        }

                        break;
                }
            }

            // If diagonal movement is enabled, check opposite directions on 8 directions.
            switch (currentDir)
            {
                case Direction.UpLeft:
                    return targetDir == Direction.DownRight || targetDir == Direction.Right ||
                           targetDir == Direction.Down;

                case Direction.UpRight:
                    return targetDir == Direction.DownLeft || targetDir == Direction.Left ||
                           targetDir == Direction.Down;

                case Direction.DownLeft:
                    return targetDir == Direction.UpRight || targetDir == Direction.Right || 
                           targetDir == Direction.Up;

                case Direction.DownRight:
                    return targetDir == Direction.UpLeft || targetDir == Direction.Left || 
                           targetDir == Direction.Up;

                default:
                    return false;
            }
        }
    }

}
