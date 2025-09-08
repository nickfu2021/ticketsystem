using System.Data;
using FluentValidation;
using TicketSystemApi.Data;
using TicketSystemApi.Dtos;
using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Services;

namespace TicketSystemApi.validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("電子信箱不可空白")
            .EmailAddress().WithMessage("電子信箱格式不正確");
        RuleFor(u => u.Password)    //這樣使用表示不符合就一次顯示所有錯誤
            .NotEmpty().WithMessage("密碼不可空白")
            .MinimumLength(8).WithMessage("密碼長度至少 8 碼")
            .Matches(@"[A-Z]").WithMessage("密碼需包含至少一個大寫英文")
            .Matches(@"[a-z]").WithMessage("密碼需包含至少一個小寫英文")
            .Matches(@"\d").WithMessage("密碼需包含至少一個數字")
            .Matches(@"[!@#$%^&*(),.?:{}|<>_\-+=\\/\[\]""';`~]").WithMessage("密碼需包含至少一個符號");
        RuleFor(u => u.Username)
            .MaximumLength(20).WithMessage("姓名長度不可超過 20 個字元");
        RuleFor(u => u.IdNumber)
            .Must(IsValidTaiwanId).WithMessage("身分證格式不正確");
        RuleFor(u => u.MobileNumber)
            .NotEmpty().WithMessage("手機號碼不可空白")
            .Matches(@"^09\d{8}$").WithMessage("手機號碼格式不正確");
        RuleFor(u => u.PostalCode)
               .Matches(@"^\d{3}$").WithMessage("郵遞區號格式須為 3 碼數字");
    }

    /// <summary>
    /// 身分證驗證
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool IsValidTaiwanId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return true;

        if (id.Length != 10)
            return false;
        /*
            1.英文字母 → 兩位數對應區碼（再拆為十位數,個位數）
            2.所有數字乘上固定權重 1(英文十位數),9(英文個位數),8,7,6,5,4,3,2,1
            3.加總後 mod 10 == 0 ⇒ 合法

            例:A123456789
        */

        id = id.ToUpper();
        string letters = "ABCDEFGHJKLMNPQRSTUVXYWZIO"; // A~Z 對應區碼代碼
        int[] codes = [10, 11, 12, 13, 14, 15, 16, 17, 34, 18,
                       19, 20, 21, 22, 35, 23, 24, 25, 26, 27,
                       28, 29, 32, 30, 31, 33]; // 對應編碼

        int index = letters.IndexOf(id[0]);
        if (index == -1) return false;

        int code = codes[index];
        int sum = (code / 10) + (code % 10) * 9;

        for (int i = 1; i < 9; i++)
        {
            if (!char.IsDigit(id[i])) return false;
            sum += (id[i] - '0') * (9 - i);
        }

        if (!char.IsDigit(id[9])) return false;
        sum += (id[9] - '0');

        return sum % 10 == 0;
    }
}