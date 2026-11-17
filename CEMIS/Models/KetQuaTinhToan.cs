using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("ket_qua_tinh_toan")]
[Index("IdDuLieuHoatDong", Name = "ket_qua_tinh_toan_id_du_lieu_hoat_dong_key", IsUnique = true)]
public partial class KetQuaTinhToan
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_du_lieu_hoat_dong")]
    public long IdDuLieuHoatDong { get; set; }

    [Column("id_to_chuc")]
    public long IdToChuc { get; set; }

    [Column("id_ky_bao_cao")]
    public long IdKyBaoCao { get; set; }

    [Column("id_he_so_phat_thai")]
    public long IdHeSoPhatThai { get; set; }

    [Column("pham_vi")]
    [StringLength(20)]
    public string PhamVi { get; set; } = null!;

    [Column("so_luong_hoat_dong")]
    [Precision(15, 4)]
    public decimal SoLuongHoatDong { get; set; }

    [Column("don_vi_tinh")]
    [StringLength(30)]
    public string DonViTinh { get; set; } = null!;

    [Column("phat_thai_co2_kg")]
    [Precision(18, 6)]
    public decimal? PhatThaiCo2Kg { get; set; }

    [Column("phat_thai_ch4_kg")]
    [Precision(18, 6)]
    public decimal? PhatThaiCh4Kg { get; set; }

    [Column("phat_thai_n2o_kg")]
    [Precision(18, 6)]
    public decimal? PhatThaiN2oKg { get; set; }

    [Column("tong_phat_thai_kg_co2e")]
    [Precision(18, 6)]
    public decimal TongPhatThaiKgCo2e { get; set; }

    [Column("tong_phat_thai_tan_co2e")]
    [Precision(18, 6)]
    public decimal TongPhatThaiTanCo2e { get; set; }

    [Column("cong_thuc_ap_dung")]
    public string? CongThucApDung { get; set; }

    [Column("ngay_tinh_toan", TypeName = "timestamp without time zone")]
    public DateTime NgayTinhToan { get; set; }

    [ForeignKey("IdDuLieuHoatDong")]
    [InverseProperty("KetQuaTinhToan")]
    public virtual DuLieuHoatDong IdDuLieuHoatDongNavigation { get; set; } = null!;

    [ForeignKey("IdHeSoPhatThai")]
    [InverseProperty("KetQuaTinhToans")]
    public virtual HeSoPhatThai IdHeSoPhatThaiNavigation { get; set; } = null!;

    [ForeignKey("IdKyBaoCao")]
    [InverseProperty("KetQuaTinhToans")]
    public virtual KyBaoCao IdKyBaoCaoNavigation { get; set; } = null!;

    [ForeignKey("IdToChuc")]
    [InverseProperty("KetQuaTinhToans")]
    public virtual ToChuc IdToChucNavigation { get; set; } = null!;
}
