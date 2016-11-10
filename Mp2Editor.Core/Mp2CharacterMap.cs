using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mp2Editor.Core
{
    public class Mp2CharacterMap
    {
        public static Dictionary<int, char> ValueToChar;

        static Mp2CharacterMap()
        {
            var dict = new Dictionary<int, char>();

            for (char i = 'a'; i <= 'z'; i++)
                dict[i] = i;

            for (char i = 'A'; i <= 'Z'; i++)
                dict[i] = i;

            for (char i = '0'; i <= '9'; i++)
                dict[i] = i;

            dict[33] = '!';
            dict[34] = '\"';
            dict[35] = '#';
            dict[36] = '$';
            dict[37] = '%';
            dict[38] = '&';
            dict[39] = '\'';
            dict[40] = '(';
            dict[41] = ')';
            dict[42] = '*';
            dict[43] = '+';
            dict[44] = ',';
            dict[45] = '-';
            dict[46] = '.';
            dict[47] = '/';
            dict[58] = ':';
            dict[59] = ';';
            dict[60] = '<';
            dict[62] = '>';
            dict[63] = '?';
            dict[32] = ' ';

            ValueToChar = dict;
        }
    }
}
