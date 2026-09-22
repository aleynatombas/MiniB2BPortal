using MiniB2B.Business.Exceptions;

namespace MiniB2B.Business.Validation;

public static class UserRules
{
    public static void ValidateProfile(string firstName, string lastName, string email, string phone, string username)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new BusinessException("Ad ve soyad zorunludur.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new BusinessException("Geçerli bir e-posta adresi giriniz.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new BusinessException("Telefon zorunludur.");
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 3)
            throw new BusinessException("Kullanıcı adı en az 3 karakter olmalıdır.");
    }

    public static void ValidatePassword(string? password, bool required)
    {
        if (!required && string.IsNullOrWhiteSpace(password))
            return;
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new BusinessException("Şifre en az 6 karakter olmalıdır.");
    }
}
