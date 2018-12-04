using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services
{
    public class TimeFormatHelper
    {
        /// <summary>
        /// 将十六进制格式表示的时间转换为十进制
        /// </summary>
        /// <param name="HexTime">十六进制格式 "yymmddhhmmss"</param>
        /// <returns>"yyyy/mm/dd hh:mm:ss"</returns>
        public string HexTimeToDecTime(string HexTime)
        {
            MathHelper mathHelper = new MathHelper();
            byte[] times = mathHelper.HexConvertToByte(HexTime);
            string result = string.Format("20{0}/{1}/{2} {3}:{4}:{5}", times[0].ToString("00"), times[1].ToString("00"), times[2].ToString("00"), times[3].ToString("00"), times[4].ToString("00"), times[5].ToString("00"));
            return result;
        }
    }
}
