using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("du_lieu_san_xuat_kinh_doanh")]
public partial class DuLieuSanXuatKinhDoanh
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_to_chuc")]
    public long IdToChuc { get; set; }

    [Column("id_ky_bao_cao")]
    public long IdKyBaoCao { get; set; }

    [Column("san_luong_san_pham")]
    [Precision(15, 2)]
    public decimal? SanLuongSanPham { get; set; }

    [Column("don_vi_san_pham")]
    [StringLength(50)]
    public string? DonViSanPham { get; set; }

    [Column("doanh_thu_trieu_dong")]
    [Precision(18, 2)]
    public decimal? DoanhThuTrieuDong { get; set; }

    [Column("so_gio_van_hanh")]
    [Precision(10, 2)]
    public decimal? SoGioVanHanh { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [ForeignKey("IdKyBaoCao")]
    [InverseProperty("DuLieuSanXuatKinhDoanhs")]
    public virtual KyBaoCao IdKyBaoCaoNavigation { get; set; } = null!;

    [ForeignKey("IdToChuc")]
    [InverseProperty("DuLieuSanXuatKinhDoanhs")]
    public virtual ToChuc IdToChucNavigation { get; set; } = null!;
}
