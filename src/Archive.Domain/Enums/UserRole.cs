namespace Archive.Domain.Enums
{
    /// <summary>
    /// أدوار النظام الثلاثة (الحسابات الافتراضية في Seed).
    /// User = قارئ، Admin/librarian = أمين مكتبة، SuperAdmin/admin = مسؤول النظام.
    /// </summary>
    public enum UserRole
    {
        /// <summary>عرض الكتب والتصنيفات فقط</summary>
        User = 0,

        /// <summary>إدارة الكتب + عرض/حذف المستخدمين</summary>
        Admin = 1,

        /// <summary>صلاحيات كاملة بما فيها إنشاء مستخدمين بأي دور</summary>
        SuperAdmin = 2
    }
}
