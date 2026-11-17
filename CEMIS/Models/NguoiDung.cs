using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("nguoi_dung")]
[Index("Email", Name = "nguoi_dung_email_key", IsUnique = true)]
[Index("TenDangNhap", Name = "nguoi_dung_ten_dang_nhap_key", IsUnique = true)]
public partial class NguoiDung
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ten_dang_nhap")]
    [StringLength(100)]
    public string TenDangNhap { get; set; } = null!;

    [Column("mat_khau_hash")]
    [StringLength(255)]
    public string MatKhauHash { get; set; } = null!;

    [Column("ho_ten")]
    [StringLength(150)]
    public string HoTen { get; set; } = null!;

    [Column("email")]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Column("so_dien_thoai")]
    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [Column("id_vai_tro")]
    public long IdVaiTro { get; set; }

    [Column("id_to_chuc")]
    public long? IdToChuc { get; set; }

    [Column("trang_thai")]
    [StringLength(50)]
    public string? TrangThai { get; set; }

    [Column("duong_dan_chu_ky_anh")]
    [StringLength(500)]
    public string? DuongDanChuKyAnh { get; set; }

    [InverseProperty("IdNguoiXuatNavigation")]
    public virtual ICollection<BaoCaoXuat> BaoCaoXuats { get; set; } = new List<BaoCaoXuat>();

    [InverseProperty("IdNguoiNhapNavigation")]
    public virtual ICollection<DuLieuHoatDong> DuLieuHoatDongs { get; set; } = new List<DuLieuHoatDong>();

    [ForeignKey("IdToChuc")]
    [InverseProperty("NguoiDungs")]
    public virtual ToChuc? IdToChucNavigation { get; set; }

    [ForeignKey("IdVaiTro")]
    [InverseProperty("NguoiDungs")]
    public virtual VaiTro IdVaiTroNavigation { get; set; } = null!;

    [InverseProperty("IdNguoiThucHienNavigation")]
    public virtual ICollection<LichSuPheDuyet> LichSuPheDuyets { get; set; } = new List<LichSuPheDuyet>();
}
