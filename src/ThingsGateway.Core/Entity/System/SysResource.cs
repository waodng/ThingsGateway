﻿namespace ThingsGateway.Core
{
    /// <summary>
    /// 资源
    ///</summary>
    [SugarTable("sys_resource", TableDescription = "资源")]
    [Tenant(SqlsugarConst.DB_Default)]
    public class SysResource : BaseEntity, ITree<SysResource>
    {

        /// <summary>
        /// 标题
        ///</summary>
        [SugarColumn(ColumnName = "Title", ColumnDescription = "标题", Length = 200)]
        public virtual string Title { get; set; }

        /// <summary>
        /// 图标
        ///</summary>
        [SugarColumn(ColumnName = "Icon", ColumnDescription = "图标", Length = 200, IsNullable = true)]
        public virtual string Icon { get; set; }


        /// <summary>
        /// 别名
        ///</summary>
        [SugarColumn(ColumnName = "Name", ColumnDescription = "别名", Length = 200, IsNullable = true)]
        public string Name { get; set; }

        /// <summary>
        /// 路径
        ///</summary>
        [SugarColumn(ColumnName = "Component", ColumnDescription = "组件", Length = 200, IsNullable = true)]
        public virtual string Component { get; set; }

        /// <summary>
        /// 分类
        ///</summary>
        [SugarColumn(ColumnName = "Category", ColumnDescription = "分类", Length = 200)]
        public MenuCategoryEnum Category { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<SysResource> Children { get; set; }

        /// <summary>
        /// 编码
        ///</summary>
        [SugarColumn(ColumnName = "Code", ColumnDescription = "编码", Length = 200, IsNullable = true)]
        public virtual string Code { get; set; }



        /// <summary>
        /// 父id
        ///</summary>
        [SugarColumn(ColumnName = "ParentId", ColumnDescription = "父id", IsNullable = true)]
        public virtual long ParentId { get; set; }

        /// <summary>
        /// 排序码
        ///</summary>
        [SugarColumn(ColumnName = "SortCode", ColumnDescription = "排序码", IsNullable = true)]
        public int SortCode { get; set; }

        /// <summary>
        /// 跳转类型
        ///</summary>
        [SugarColumn(ColumnName = "TargetType", ColumnDescription = "跳转类型", Length = 200, IsNullable = true)]
        public virtual TargetTypeEnum TargetType { get; set; }

    }
}