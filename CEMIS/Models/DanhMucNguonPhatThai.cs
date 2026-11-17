using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("danh_muc_nguon_phat_thai")]
[Index("MaNguon", Name = "danh_muc_nguon_phat_thai_ma_nguon_key", IsUnique = true)]
public partial class DanhMucNguonPhatThai
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ma_nguon")]
    [StringLength(50)]
    public string MaNguon { get; set; } = null!;

    [Column("ten_nguon")]
    [StringLength(255)]
    public string TenNguon { get; set; } = null!;

    [Column("pham_vi")]
    [StringLength(20)]
    public string PhamVi { get; set; } = null!;

    [Column("phan_loai")]
    [StringLength(100)]
    public string PhanLoai { get; set; } = null!;

    [Column("don_vi_tinh_chuan")]
    [StringLength(30)]
    public string DonViTinhChuan { get; set; } = null!;

    [Column("mo_ta")]
    public string? MoTa { get; set; }

    [InverseProperty("IdNguonPhatThaiNavigation")]
    public virtual ICollection<DinhMucCanhBao> DinhMucCanhBaos { get; set; } = new List<DinhMucCanhBao>();

    [InverseProperty("IdNguonPhatThaiNavigation")]
    public virtual ICollection<DuLieuHoatDong> DuLieuHoatDongs { get; set; } = new List<DuLieuHoatDong>();

    [InverseProperty("IdNguonPhatThaiNavigation")]
    public virtual ICollection<HeSoPhatThai> HeSoPhatThais { get; set; } = new List<HeSoPhatThai>();
}
