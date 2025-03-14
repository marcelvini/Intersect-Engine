using MessagePack;
using System;

namespace Intersect.Network.Packets.Server
{
    [MessagePackObject]
    public partial class EquipmentFragment
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public Color RenderColor { get; set; }
    }

    [MessagePackObject]
    public partial class CharacterPacket : IntersectPacket
    {
        //Parameterless Constructor for MessagePack
        public CharacterPacket()
        {
        }

        public CharacterPacket(
            Guid id,
            string name,
            string sprite,
            string face,
            int level,
            string className,
            EquipmentFragment[] equipment,
            bool hideAllPaperdolls
        )
        {
            Id = id;
            Name = name;
            Sprite = sprite;
            Face = face;
            Level = level;
            ClassName = className;
            Equipment = equipment;
            HideAllPaperdolls = hideAllPaperdolls;
        }

        [Key(0)]
        public Guid Id { get; set; }

        [Key(1)]
        public string Name { get; set; }

        [Key(2)]
        public string Sprite { get; set; }

        [Key(3)]
        public string Face { get; set; }

        [Key(4)]
        public int Level { get; set; }

        [Key(5)]
        public string ClassName { get; set; }

        [Key(6)]
        public EquipmentFragment[] Equipment { get; set; }

        [Key(7)]
        public bool HideAllPaperdolls { get; set; }

    }

}
