using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPIController
{
    public class _writeBuffer
    {
        public byte[] buffer = new byte[3];

        public _writeBuffer(int channel, float volts)
        {
            try
            {
                buffer[0] = BuildChannel(channel);
                var tmp = BuildVoltageValue(volts);
                buffer[1] = tmp[0];
                buffer[2] = tmp[1];
            }
            catch(Exception ex)
            {
                var error = ex.Message.ToString();
            }
        }
        private Byte BuildChannel(int channel)
        {
            Byte result;
            if (channel < 8)
            {
                Byte val = Convert.ToByte(channel);
                result = Constants.ad5360_Mode | Constants.ad5360_Group1;
                result += val;
            }
            else
            {
                Byte val = Convert.ToByte(channel - 8);
                result = Constants.ad5360_Mode | Constants.ad5360_Group2;
                result += val;

            }

            return result;
        }

        private Byte[] BuildVoltageValue(float volts)
        {
            Byte[] result = new Byte[2];
            var value = ((volts) * 65536);
            value = value / 10;
            value = value + 32768;
            try
            {
                UInt16 iValue = Convert.ToUInt16(value);
                var bValue = BitConverter.GetBytes(iValue);
                result[0] = bValue[1];
                result[1] = bValue[0];
            }
            catch(Exception ex)
            {
                var tmp = ex.Message.ToString();
            }
            return result;
        }
    }
    public class _TimeSlice
    {
        public List<_writeBuffer> timeSlice = new List<_writeBuffer>();
    }

}
