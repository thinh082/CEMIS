using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("lich_su_phe_duyet")]
public partial class LichSuPheDuyet
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_du_lieu_hoat_dong")]
    public long IdDuLieuHoatDong { get; set; }

    [Column("id_nguoi_thuc_hien")]
    public long IdNguoiThucHien { get; set; }

    [Column("hanh_dong")]
    [StringLength(50)]
    public string HanhDong { get; set; } = null!;

    [Column("ngay_thuc_hien", TypeName = "timestamp without time zone")]
    public DateTime NgayThucHien { get; set; }

    [Column("ly_do_tu_choi")]
    public string? LyDoTuChoi { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [ForeignKey("IdDuLieuHoatDong")]
    [InverseProperty("LichSuPheDuyets")]
    public virtual DuLieuHoatDong IdDuLieuHoatDongNavigation { get; set; } = null!;

    [ForeignKey("IdNguoiThucHien")]
    [InverseProperty("LichSuPheDuyets")]
    public virtual NguoiDung IdNguoiThucHienNavigation { get; set; } = null!;
}
