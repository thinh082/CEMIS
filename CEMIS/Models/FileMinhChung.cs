using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("file_minh_chung")]
public partial class FileMinhChung
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_du_lieu_hoat_dong")]
    public long IdDuLieuHoatDong { get; set; }

    [Column("ten_file")]
    [StringLength(255)]
    public string TenFile { get; set; } = null!;

    [Column("duong_dan_file")]
    public string DuongDanFile { get; set; } = null!;

    [Column("loai_file")]
    [StringLength(50)]
    public string? LoaiFile { get; set; }

    [Column("dung_luong_kb")]
    public long? DungLuongKb { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [ForeignKey("IdDuLieuHoatDong")]
    [InverseProperty("FileMinhChungs")]
    public virtual DuLieuHoatDong IdDuLieuHoatDongNavigation { get; set; } = null!;
}
