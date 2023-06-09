﻿namespace ThingsGateway.Application
{
    /// <summary>
    /// 编辑个人信息参数
    /// </summary>
    public class UpdateInfoInput : SysUser
    {
        /// <summary>
        /// Id
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }
    /// <summary>
    /// 修改密码
    /// </summary>
    public class PasswordInfoInput : BaseIdInput, IValidatableObject
    {
        [Description("旧密码")]
        [Required(ErrorMessage = "不能为空")]
        public string OldPassword { get; set; }
        [Description("新密码")]
        [Required(ErrorMessage = "不能为空")]
        public string NewPassword { get; set; }
        [Description("确认密码")]
        [Required(ErrorMessage = "不能为空")]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NewPassword != ConfirmPassword)
                yield return new ValidationResult("两次密码不一致", new[] { nameof(ConfirmPassword) });
        }
    }
}
