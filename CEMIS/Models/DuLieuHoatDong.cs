using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("du_lieu_hoat_dong")]
public partial class DuLieuHoatDong
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_to_chuc")]
    public long IdToChuc { get; set; }

    [Column("id_ky_bao_cao")]
    public long IdKyBaoCao { get; set; }

    [Column("id_nguon_phat_thai")]
    public long IdNguonPhatThai { get; set; }

    [Column("id_nguoi_nhap")]
    public long IdNguoiNhap { get; set; }

    [Column("so_luong_nhap")]
    [Precision(15, 4)]
    public decimal SoLuongNhap { get; set; }

    [Column("don_vi_nhap")]
    [StringLength(30)]
    public string DonViNhap { get; set; } = null!;

    [Column("he_so_quy_doi")]
    [Precision(15, 6)]
    public decimal? HeSoQuyDoi { get; set; }

    [Column("so_luong_quy_chuan")]
    [Precision(15, 4)]
    public decimal SoLuongQuyChuan { get; set; }

    [Column("ngay_su_dung")]
    public DateOnly? NgaySuDung { get; set; }

    [Column("so_hoa_don")]
    [StringLength(100)]
    public string? SoHoaDon { get; set; }

    [Column("ngay_hoa_don")]
    public DateOnly? NgayHoaDon { get; set; }

    [Column("nha_cung_cap")]
    [StringLength(255)]
    public string? NhaCungCap { get; set; }

    [Column("trang_thai_duyet")]
    [StringLength(50)]
    public string? TrangThaiDuyet { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [InverseProperty("IdDuLieuHoatDongNavigation")]
    public virtual ICollection<FileMinhChung> FileMinhChungs { get; set; } = new List<FileMinhChung>();

    [ForeignKey("IdKyBaoCao")]
    [InverseProperty("DuLieuHoatDongs")]
    public virtual KyBaoCao IdKyBaoCaoNavigation { get; set; } = null!;

    [ForeignKey("IdNguoiNhap")]
    [InverseProperty("DuLieuHoatDongs")]
    public virtual NguoiDung IdNguoiNhapNavigation { get; set; } = null!;

    [ForeignKey("IdNguonPhatThai")]
    [InverseProperty("DuLieuHoatDongs")]
    public virtual DanhMucNguonPhatThai IdNguonPhatThaiNavigation { get; set; } = null!;

    [ForeignKey("IdToChuc")]
    [InverseProperty("DuLieuHoatDongs")]
    public virtual ToChuc IdToChucNavigation { get; set; } = null!;

    [InverseProperty("IdDuLieuHoatDongNavigation")]
    public virtual KetQuaTinhToan? KetQuaTinhToan { get; set; }

    [InverseProperty("IdDuLieuHoatDongNavigation")]
    public virtual ICollection<LichSuPheDuyet> LichSuPheDuyets { get; set; } = new List<LichSuPheDuyet>();
}
