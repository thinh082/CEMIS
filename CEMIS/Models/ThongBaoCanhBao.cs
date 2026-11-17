using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("thong_bao_canh_bao")]
public partial class ThongBaoCanhBao
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_to_chuc")]
    public long IdToChuc { get; set; }

    [Column("id_ky_bao_cao")]
    public long? IdKyBaoCao { get; set; }

    [Column("id_dinh_muc")]
    public long? IdDinhMuc { get; set; }

    [Column("tieu_de")]
    [StringLength(255)]
    public string TieuDe { get; set; } = null!;

    [Column("noi_dung")]
    public string NoiDung { get; set; } = null!;

    [Column("gia_tri_thuc_te")]
    [Precision(15, 4)]
    public decimal GiaTriThucTe { get; set; }

    [Column("gia_tri_dinh_muc")]
    [Precision(15, 4)]
    public decimal GiaTriDinhMuc { get; set; }

    [Column("phan_tram_vuot")]
    [Precision(10, 2)]
    public decimal PhanTramVuot { get; set; }

    [Column("muc_do")]
    [StringLength(50)]
    public string MucDo { get; set; } = null!;

    [Column("ngay_phat_hien", TypeName = "timestamp without time zone")]
    public DateTime NgayPhatHien { get; set; }

    [Column("da_xu_ly")]
    public bool? DaXuLy { get; set; }

    [ForeignKey("IdDinhMuc")]
    [InverseProperty("ThongBaoCanhBaos")]
    public virtual DinhMucCanhBao? IdDinhMucNavigation { get; set; }

    [ForeignKey("IdKyBaoCao")]
    [InverseProperty("ThongBaoCanhBaos")]
    public virtual KyBaoCao? IdKyBaoCaoNavigation { get; set; }

    [ForeignKey("IdToChuc")]
    [InverseProperty("ThongBaoCanhBaos")]
    public virtual ToChuc IdToChucNavigation { get; set; } = null!;
}
