﻿namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 数据类型
/// </summary>
public enum DataTypeEnum
{
    Object,

    Bcd,
    DateTime,
    String,

    Bool,
    Byte,
    SByte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float,
    Double,
}
/// <summary>
/// 数据类型信息
/// </summary>
public static class DataTypeExtension
{

    public static Type GetNetType(this DataTypeEnum coreDataType)
    {
        switch (coreDataType)
        {
            case DataTypeEnum.Object:
                return typeof(object);
            case DataTypeEnum.Bcd:
                return typeof(string);
            case DataTypeEnum.DateTime:
                return typeof(DateTime);
            case DataTypeEnum.String:
                return typeof(string);
            case DataTypeEnum.Bool:
                return typeof(bool);
            case DataTypeEnum.Byte:
                return typeof(byte);
            case DataTypeEnum.SByte:
                return typeof(sbyte);
            case DataTypeEnum.Int16:
                return typeof(short);
            case DataTypeEnum.UInt16:
                return typeof(ushort);
            case DataTypeEnum.Int32:
                return typeof(int);
            case DataTypeEnum.UInt32:
                return typeof(uint);
            case DataTypeEnum.Int64:
                return typeof(long);
            case DataTypeEnum.UInt64:
                return typeof(ulong);
            case DataTypeEnum.Float:
                return typeof(float);
            case DataTypeEnum.Double:
                return typeof(double);
            default:
                return typeof(string);
        }
    }
    public static int GetByteLength(this DataTypeEnum coreDataType)
    {
        switch (coreDataType)
        {
            case DataTypeEnum.Object:
                return 0;
            case DataTypeEnum.Bcd:
                return 0;
            case DataTypeEnum.DateTime:
                return 0;
            case DataTypeEnum.String:
                return 0;
            case DataTypeEnum.Bool:
                return 1;
            case DataTypeEnum.Byte:
                return 1;
            case DataTypeEnum.SByte:
                return 1;
            case DataTypeEnum.Int16:
                return 2;
            case DataTypeEnum.UInt16:
                return 2;
            case DataTypeEnum.Int32:
                return 4;
            case DataTypeEnum.UInt32:
                return 4;
            case DataTypeEnum.Int64:
                return 8;
            case DataTypeEnum.UInt64:
                return 8;
            case DataTypeEnum.Float:
                return 4;
            case DataTypeEnum.Double:
                return 8;
            default:
                return 0;
        }
    }


}