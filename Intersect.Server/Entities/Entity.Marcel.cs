using System;
using System.Collections.Generic;
using Intersect.GameObjects;
using Intersect.Utilities;

namespace Intersect.Server.Entities
{
    public class ChainSpellData
    {
        public Entity Target { get; set; }

        public long TimeToCast { get; set; }
    }

    public abstract partial class Entity
    {
        public Dictionary<Guid, ChainSpellData> ChainCastSpells = new Dictionary<Guid, ChainSpellData>();

        public void AddChainSpell(SpellBase spell, Entity target)
        {
            if (spell == null)
            {
                return;
            }

            if (spell.Combat.ChainSpellId == null)
            {
                return;
            }

            if (ChainCastSpells.TryGetValue(spell.Combat.ChainSpellId, out var _))
            {
                return;
            }

            ChainSpellData data = new ChainSpellData()
            {
                Target = target,
                TimeToCast = Timing.Global.Milliseconds + spell.Combat.ChainSpellDelay
            };

            ChainCastSpells.Add(spell.Combat.ChainSpellId, data);
        }

        public void CheckChainSpells()
        {
            if (ChainCastSpells.Count == 0)
            {
                return;
            }

            Dictionary<Guid, ChainSpellData> SpellList = new Dictionary<Guid, ChainSpellData>(ChainCastSpells);

            foreach (KeyValuePair<Guid, ChainSpellData> entry in SpellList)
            {
                var spell = SpellBase.Get(entry.Key);

                if (spell == null)
                {
                    ChainCastSpells.Remove(entry.Key);
                    continue;
                }

                if (entry.Value.TimeToCast < Timing.Global.Milliseconds)
                {
                    if (CanCastSpell(spell, entry.Value.Target, false, out var _))
                    {
                        CastTarget = entry.Value.Target;
                        CastSpell(entry.Key);
                    }

                    ChainCastSpells.Remove(entry.Key);
                }
            }
        }
    }
}
