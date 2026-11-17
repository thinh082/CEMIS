using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("to_chuc")]
[Index("MaToChuc", Name = "to_chuc_ma_to_chuc_key", IsUnique = true)]
public partial class ToChuc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("ma_to_chuc")]
    [StringLength(50)]
    public string MaToChuc { get; set; } = null!;

    [Column("ten_to_chuc")]
    [StringLength(255)]
    public string TenToChuc { get; set; } = null!;

    [Column("loai_to_chuc")]
    [StringLength(50)]
    public string LoaiToChuc { get; set; } = null!;

    [Column("id_to_chuc_cha")]
    public long? IdToChucCha { get; set; }

    [Column("dia_chi")]
    public string? DiaChi { get; set; }

    [Column("dien_tich_san_m2")]
    [Precision(15, 2)]
    public decimal? DienTichSanM2 { get; set; }

    [Column("so_nhan_vien")]
    public int? SoNhanVien { get; set; }

    [Column("trang_thai")]
    [StringLength(50)]
    public string? TrangThai { get; set; }

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<BaoCaoXuat> BaoCaoXuats { get; set; } = new List<BaoCaoXuat>();

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<DinhMucCanhBao> DinhMucCanhBaos { get; set; } = new List<DinhMucCanhBao>();

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<DuLieuHoatDong> DuLieuHoatDongs { get; set; } = new List<DuLieuHoatDong>();

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<DuLieuSanXuatKinhDoanh> DuLieuSanXuatKinhDoanhs { get; set; } = new List<DuLieuSanXuatKinhDoanh>();

    [ForeignKey("IdToChucCha")]
    [InverseProperty("InverseIdToChucChaNavigation")]
    public virtual ToChuc? IdToChucChaNavigation { get; set; }

    [InverseProperty("IdToChucChaNavigation")]
    public virtual ICollection<ToChuc> InverseIdToChucChaNavigation { get; set; } = new List<ToChuc>();

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<KetQuaTinhToan> KetQuaTinhToans { get; set; } = new List<KetQuaTinhToan>();

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<KyBaoCao> KyBaoCaos { get; set; } = new List<KyBaoCao>();

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();

    [InverseProperty("IdToChucNavigation")]
    public virtual ICollection<ThongBaoCanhBao> ThongBaoCanhBaos { get; set; } = new List<ThongBaoCanhBao>();
}
