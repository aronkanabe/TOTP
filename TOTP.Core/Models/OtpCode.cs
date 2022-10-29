using System.Security.Cryptography;
using TOTP.Core.Interfaces;

namespace TOTP.Core.Models;

public class OtpCode
{
    public OtpCode(string code)
    {
        Code = code;
    }

    public string Code { get; }

    protected bool Equals(OtpCode other)
    {
        return Code == other.Code;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((OtpCode) obj);
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode();
    }

    public static bool operator ==(OtpCode? left, OtpCode? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(OtpCode? left, OtpCode? right)
    {
        return !Equals(left, right);
    }
}