using System.Data;
using FluentValidation;
using TicketSystemApi.Data;
using TicketSystemApi.Dtos;
using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Services;

namespace TicketSystemApi.validators;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{

    public UserUpdateDtoValidator()
    {

        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("姓名不可空白")
            .MaximumLength(20).WithMessage("姓名長度不可超過 20 個字元");
        RuleFor(u => u.Password)    //這樣使用表示不符合就一次顯示所有錯誤
            .NotEmpty().WithMessage("密碼不可空白")
            .MinimumLength(8).WithMessage("密碼長度至少 8 碼")
            .Matches(@"[A-Z]").WithMessage("密碼需包含至少一個大寫英文")
            .Matches(@"[a-z]").WithMessage("密碼需包含至少一個小寫英文")
            .Matches(@"\d").WithMessage("密碼需包含至少一個數字")
            .Matches(@"[!@#$%^&*(),.?:{}|<>_\-+=\\/\[\]""';`~]").WithMessage("密碼需包含至少一個符號");
        RuleFor(u => u.PhoneNumber)
            .NotEmpty().WithMessage("聯絡電話不可空白")
            .Matches(@"^(\d{2,4}-\d{6,8}|\d{10})$")
            .WithMessage("聯絡電話格式須為市話 (含區碼) 或 10 碼手機號碼");
        RuleFor(u => u.MobileNumber)
            .NotEmpty().WithMessage("手機號碼不可空白")
            .Matches(@"^09\d{8}$").WithMessage("手機號碼格式不正確");
        RuleFor(u => u.PostalCode)
               .NotEmpty().WithMessage("郵遞區號不可空白")
               .Matches(@"^\d{3}$").WithMessage("郵遞區號格式須為 3 碼數字");
        RuleFor(u => u.City)
            .NotEmpty().WithMessage("縣市不可空白");
        RuleFor(u => u.District)
            .NotEmpty().WithMessage("鄉鎮區不可空白");
        RuleFor(u => u.AddressDetail)
            .NotEmpty().WithMessage("詳細地址不可空白");
        
    }

}