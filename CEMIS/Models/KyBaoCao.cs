using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("ky_bao_cao")]
public partial class KyBaoCao
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_to_chuc")]
    public long IdToChuc { get; set; }

    [Column("ten_ky")]
    [StringLength(100)]
    public string TenKy { get; set; } = null!;

    [Column("thang")]
    public int? Thang { get; set; }

    [Column("quy")]
    public int? Quy { get; set; }

    [Column("nam")]
    public int Nam { get; set; }

    [Column("ngay_bat_dau")]
    public DateOnly NgayBatDau { get; set; }

    [Column("ngay_ket_thuc")]
    public DateOnly NgayKetThuc { get; set; }

    [Column("trang_thai_khoa_so")]
    [StringLength(50)]
    public string? TrangThaiKhoaSo { get; set; }

    [InverseProperty("IdKyBaoCaoNavigation")]
    public virtual ICollection<BaoCaoXuat> BaoCaoXuats { get; set; } = new List<BaoCaoXuat>();

    [InverseProperty("IdKyBaoCaoNavigation")]
    public virtual ICollection<DuLieuHoatDong> DuLieuHoatDongs { get; set; } = new List<DuLieuHoatDong>();

    [InverseProperty("IdKyBaoCaoNavigation")]
    public virtual ICollection<DuLieuSanXuatKinhDoanh> DuLieuSanXuatKinhDoanhs { get; set; } = new List<DuLieuSanXuatKinhDoanh>();

    [ForeignKey("IdToChuc")]
    [InverseProperty("KyBaoCaos")]
    public virtual ToChuc IdToChucNavigation { get; set; } = null!;

    [InverseProperty("IdKyBaoCaoNavigation")]
    public virtual ICollection<KetQuaTinhToan> KetQuaTinhToans { get; set; } = new List<KetQuaTinhToan>();

    [InverseProperty("IdKyBaoCaoNavigation")]
    public virtual ICollection<ThongBaoCanhBao> ThongBaoCanhBaos { get; set; } = new List<ThongBaoCanhBao>();
}
