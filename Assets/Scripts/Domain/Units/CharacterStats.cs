namespace TacticFantasy.Domain.Units
{
    public struct CharacterStats
    {
        public int HP;
        public int STR;
        public int MAG;
        public int SKL;
        public int SPD;
        public int LCK;
        public int DEF;
        public int RES;
        public int MOV;

        public CharacterStats(int hp, int str, int mag, int skl, int spd, int lck, int def, int res, int mov)
        {
            HP = hp;
            STR = str;
            MAG = mag;
            SKL = skl;
            SPD = spd;
            LCK = lck;
            DEF = def;
            RES = res;
            MOV = mov;
        }

        public static CharacterStats operator +(CharacterStats a, CharacterStats b)
        {
            return new CharacterStats(
                a.HP + b.HP,
                a.STR + b.STR,
                a.MAG + b.MAG,
                a.SKL + b.SKL,
                a.SPD + b.SPD,
                a.LCK + b.LCK,
                a.DEF + b.DEF,
                a.RES + b.RES,
                a.MOV + b.MOV
            );
        }
    }
}
