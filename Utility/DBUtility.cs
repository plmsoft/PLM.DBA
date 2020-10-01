using System;
using System.Data.SqlTypes;

namespace PLM.DBA.Utility
{
	public class DBUtility
	{
		public static SqlString CheckStringParamValue(string value)
		{
			return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value) ? SqlString.Null : new SqlString(value);
		}

		public static SqlDateTime CheckDateTimeParamValue(DateTime value)
		{
			return DateTime.MinValue == value ? SqlDateTime.Null : new SqlDateTime(value);
		}

		public static SqlInt32 CheckIntParamValue(int value)
		{
			return new SqlInt32(value);
		}

		public static SqlInt32 CheckIntNullParamValue(int value)
		{
			return value < 0 ? SqlInt32.Null : new SqlInt32(value);
		}

		public static SqlBoolean CheckBoolParamValue(bool value)
		{
			return new SqlBoolean(value);
		}

		public static SqlDecimal CheckDecimalParamValue(decimal value)
		{
			return new SqlDecimal(value);
		}

		public static SqlDecimal CheckDecimalNullParamValue(decimal value)
		{
			return value < 0 ? SqlDecimal.Null : new SqlDecimal(value);
		}

		public static SqlBytes CheckByteParamValue(byte[] value)
        {
            return value == null ? SqlBytes.Null : new SqlBytes(value);
        }
    }
}
