namespace RPFLib.Common
{
    public static class Hasher
    {
        public static uint Hash(string str)
        {
            uint value = 0, temp;
            var index = 0;
            var quoted = false;

            if (str[index] == '"')
            {
                quoted = true;
                index++;
            }

            str = str.ToLower();

            for (; index < str.Length; index++)
            {
                var v = str[index];

                if (quoted && (v == '"')) break;

                if (v == '\\')
                    v = '/';

                temp = v;
                temp = temp + value;
                value = temp << 10;
                temp += value;
                value = temp >> 6;
                value = value ^ temp;
            }

            temp = value << 3;
            temp = value + temp;
            var temp2 = temp >> 11;
            temp = temp2 ^ temp;
            temp2 = temp << 15;

            value = temp2 + temp;

            if (value < 2) value += 2;

            return value;
        }
    }
}