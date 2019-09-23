namespace Buffs.player {
    public class HolyStack : CharacterBuff {
        
        public HolyStack(): base("holy_stack", true, "explanation placeholder", 5) {
            count = 1;
        }
        
    }
}