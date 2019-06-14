using System;

namespace Tiny {
	public static class Json {

		public const string Version = "1.0";

		public static T Decode<T>(string json) {
			if (string.IsNullOrEmpty(json)) return default(T);
			object jsonObj = JsonParser.ParseValue(json);
			if (jsonObj == null) return default(T);
			return JsonMapper.DecodeJsonObject<T>(jsonObj);
		}
		
		public static string Encode(object value) {
			JsonBuilder builder = new JsonBuilder();
			JsonMapper.EncodeValue(value, builder);
			return builder.ToString();
		}

		public static bool IsNullable(this Type type) {
			return Nullable.GetUnderlyingType(type) != null || !type.IsPrimitive;
		}

		public static bool IsNumeric(this Type type) {
			if (type.IsEnum) return false;
			switch (Type.GetTypeCode(type)) {
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Single:
				return true;
			case TypeCode.Object:
				Type underlyingType = Nullable.GetUnderlyingType(type);
				return underlyingType != null && underlyingType.IsNumeric();
			default:
				return false;
			}
		}
	}
}

