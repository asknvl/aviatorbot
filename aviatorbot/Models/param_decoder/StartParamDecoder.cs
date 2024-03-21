using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botservice.Models.param_decoder
{
    public static class StartParamDecoder
    {

        static string getSource(string input)
        {

            string res;

            switch (input)
            {

                case "0":
                    res = "IND_TG";
                    break;
                case "1":
                    res = "IND_INSTA";
                    break;
                case "4":
                    res = "COL_TG";
                    break;
                default:
                    res = "UNKNOWN";
                    break;
            }

            return res;
        }

        public static DecodedParam Decode(string input)
        {
            DecodedParam res = new DecodedParam();

            if (input == null)
                return res;

            string h_source;
            string s_num;


            input = input.Trim();

            if (input.Length >= 8)
            {
                h_source = input.Substring(0, 1);
                res.source = getSource(h_source);
                s_num = input.Substring(1, 3);

                try
                {
                    res.num = Convert.ToInt32("0x" + s_num, 16);
                }
                catch (Exception ex)
                {
                    res.num = -1;
                }

                switch (input.Length)
                {
                    case 8:
                        res.buyer = input.Substring(4, 2);
                        res.closer = input.Substring(6, 2);
                        break;
                    case 11:
                        res.buyer = input.Substring(4, 5);
                        res.closer = input.Substring(9, 2);
                        break;
                    case 13:
                        res.buyer = input.Substring(4, 5);
                        res.closer = input.Substring(9, 5);
                        break;
                }

            }

            return res;
        }
    }

    public class DecodedParam
    {
        public string buyer { get; set; } = "UNKNOWN";
        public string closer { get; set; } = "UNKNOWN";
        public string source { get; set; } = "UNKNOWN";
        public int num { get; set; } = -1;
    }
}
