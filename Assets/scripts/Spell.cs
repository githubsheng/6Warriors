public class Spell
{
    public int physicalAttackStrengh;
    public int magicalAttackPower;
    public int magicType;
    public int hpHeal;
    public int manaHeal;
    public string name;
    public PlayerBuff buff;
    public float spellEffectiveRange;

    public static readonly int MAGIC_TYPE_FIRE = 1;
    public static readonly int MAGIC_TYPE_ICE = 2;
}