using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

[Table("dinh_muc_canh_bao")]
public partial class DinhMucCanhBao
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("id_to_chuc")]
    public long IdToChuc { get; set; }

    [Column("id_nguon_phat_thai")]
    public long IdNguonPhatThai { get; set; }

    [Column("nam_ap_dung")]
    public int NamApDung { get; set; }

    [Column("nguong_tieu_thu_thang")]
    [Precision(15, 4)]
    public decimal? NguongTieuThuThang { get; set; }

    [Column("nguong_phat_thai_thang_tco2e")]
    [Precision(15, 4)]
    public decimal? NguongPhatThaiThangTco2e { get; set; }

    [Column("muc_do_canh_bao")]
    [StringLength(50)]
    public string? MucDoCanhBao { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [ForeignKey("IdNguonPhatThai")]
    [InverseProperty("DinhMucCanhBaos")]
    public virtual DanhMucNguonPhatThai IdNguonPhatThaiNavigation { get; set; } = null!;

    [ForeignKey("IdToChuc")]
    [InverseProperty("DinhMucCanhBaos")]
    public virtual ToChuc IdToChucNavigation { get; set; } = null!;

    [InverseProperty("IdDinhMucNavigation")]
    public virtual ICollection<ThongBaoCanhBao> ThongBaoCanhBaos { get; set; } = new List<ThongBaoCanhBao>();
}
