using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Services;
namespace ProtocolFamily.Modbus
{
    public class Modbus
    {
        const string WriteFunctionCode = "10";
        const string ReadFunctionCode = "03";

        private bool flag;
        public bool Flag
        {
            get
            {
                return flag;
            }

            set
            {
                flag = value;
            }
        }

        private string slaveId;
        public string SlaveId
        {
            get
            {
                return slaveId;
            }
            set
            {
                slaveId = value;
            }
        }

        private string address;
        public string Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
            }
        }

        public string ReceiveData
        {
            get
            {
                return receiveData;
            }

            set
            {
                receiveData = value;
            }
        }
        private string receiveData;

        /// <summary>
        /// 生成发送数据
        /// </summary>
        /// <param name="sendData"></param>
        /// <returns></returns>
        public string CreateModbusSendData(byte[] sendData)
        {
            MathHelper math = new MathHelper();
            string count = math.ByteConvertToHex((byte)(sendData.Length/2));
            count = count.PadLeft(4, '0');
            string length  = math.ByteConvertToHex((byte)sendData.Length);
            string data = this.SlaveId + WriteFunctionCode + Address + count + length + math.ByteConvertToHex(sendData);
            data += CRC.ToModbusCRC16(data);
            return data;
        }
        /// <summary>
        /// 从 从机读取数据
        /// </summary>
        /// <param name="dataCount">读取字节个数</param>
        /// <returns></returns>
        public string CreateModbusSendDataForRead(Int16 dataCount)
        {
            MathHelper math = new MathHelper();
            string count = math.ByteConvertToHex((byte)(dataCount/2));
            count = count.PadLeft(4, '0');
            string data = this.SlaveId + ReadFunctionCode + Address + count;
            data += CRC.ToModbusCRC16(data);
            return data;
        }
    }
}
