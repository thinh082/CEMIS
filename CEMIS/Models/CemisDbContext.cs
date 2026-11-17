using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Models;

public partial class CemisDbContext : DbContext
{
    public CemisDbContext()
    {
    }

    public CemisDbContext(DbContextOptions<CemisDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaoCaoXuat> BaoCaoXuats { get; set; }

    public virtual DbSet<DanhMucNguonPhatThai> DanhMucNguonPhatThais { get; set; }

    public virtual DbSet<DinhMucCanhBao> DinhMucCanhBaos { get; set; }

    public virtual DbSet<DuLieuHoatDong> DuLieuHoatDongs { get; set; }

    public virtual DbSet<DuLieuSanXuatKinhDoanh> DuLieuSanXuatKinhDoanhs { get; set; }

    public virtual DbSet<FileMinhChung> FileMinhChungs { get; set; }

    public virtual DbSet<HeSoGwp> HeSoGwps { get; set; }

    public virtual DbSet<HeSoPhatThai> HeSoPhatThais { get; set; }

    public virtual DbSet<KetQuaTinhToan> KetQuaTinhToans { get; set; }

    public virtual DbSet<KyBaoCao> KyBaoCaos { get; set; }

    public virtual DbSet<LichSuPheDuyet> LichSuPheDuyets { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<ThongBaoCanhBao> ThongBaoCanhBaos { get; set; }

    public virtual DbSet<ToChuc> ToChucs { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaoCaoXuat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bao_cao_xuat_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.NgayXuat).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdKyBaoCaoNavigation).WithMany(p => p.BaoCaoXuats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bao_cao_xuat_id_ky_bao_cao_fkey");

            entity.HasOne(d => d.IdNguoiXuatNavigation).WithMany(p => p.BaoCaoXuats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bao_cao_xuat_id_nguoi_xuat_fkey");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.BaoCaoXuats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bao_cao_xuat_id_to_chuc_fkey");
        });

        modelBuilder.Entity<DanhMucNguonPhatThai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("danh_muc_nguon_phat_thai_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<DinhMucCanhBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dinh_muc_canh_bao_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.MucDoCanhBao).HasDefaultValueSql("'canh_bao'::character varying");

            entity.HasOne(d => d.IdNguonPhatThaiNavigation).WithMany(p => p.DinhMucCanhBaos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dinh_muc_canh_bao_id_nguon_phat_thai_fkey");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.DinhMucCanhBaos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dinh_muc_canh_bao_id_to_chuc_fkey");
        });

        modelBuilder.Entity<DuLieuHoatDong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("du_lieu_hoat_dong_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.HeSoQuyDoi).HasDefaultValueSql("1.0");
            entity.Property(e => e.TrangThaiDuyet).HasDefaultValueSql("'cho_duyet'::character varying");

            entity.HasOne(d => d.IdKyBaoCaoNavigation).WithMany(p => p.DuLieuHoatDongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("du_lieu_hoat_dong_id_ky_bao_cao_fkey");

            entity.HasOne(d => d.IdNguoiNhapNavigation).WithMany(p => p.DuLieuHoatDongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("du_lieu_hoat_dong_id_nguoi_nhap_fkey");

            entity.HasOne(d => d.IdNguonPhatThaiNavigation).WithMany(p => p.DuLieuHoatDongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("du_lieu_hoat_dong_id_nguon_phat_thai_fkey");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.DuLieuHoatDongs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("du_lieu_hoat_dong_id_to_chuc_fkey");
        });

        modelBuilder.Entity<DuLieuSanXuatKinhDoanh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("du_lieu_san_xuat_kinh_doanh_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.IdKyBaoCaoNavigation).WithMany(p => p.DuLieuSanXuatKinhDoanhs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("du_lieu_san_xuat_kinh_doanh_id_ky_bao_cao_fkey");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.DuLieuSanXuatKinhDoanhs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("du_lieu_san_xuat_kinh_doanh_id_to_chuc_fkey");
        });

        modelBuilder.Entity<FileMinhChung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("file_minh_chung_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.IdDuLieuHoatDongNavigation).WithMany(p => p.FileMinhChungs).HasConstraintName("file_minh_chung_id_du_lieu_hoat_dong_fkey");
        });

        modelBuilder.Entity<HeSoGwp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("he_so_gwp_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.GwpCo2).HasDefaultValueSql("1.0");
        });

        modelBuilder.Entity<HeSoPhatThai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("he_so_phat_thai_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.ApDung).HasDefaultValue(true);
            entity.Property(e => e.HeSoCh4).HasDefaultValueSql("0");
            entity.Property(e => e.HeSoCo2).HasDefaultValueSql("0");
            entity.Property(e => e.HeSoN2o).HasDefaultValueSql("0");

            entity.HasOne(d => d.IdGwpNavigation).WithMany(p => p.HeSoPhatThais).HasConstraintName("he_so_phat_thai_id_gwp_fkey");

            entity.HasOne(d => d.IdNguonPhatThaiNavigation).WithMany(p => p.HeSoPhatThais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("he_so_phat_thai_id_nguon_phat_thai_fkey");
        });

        modelBuilder.Entity<KetQuaTinhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ket_qua_tinh_toan_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.NgayTinhToan).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.PhatThaiCh4Kg).HasDefaultValueSql("0");
            entity.Property(e => e.PhatThaiCo2Kg).HasDefaultValueSql("0");
            entity.Property(e => e.PhatThaiN2oKg).HasDefaultValueSql("0");

            entity.HasOne(d => d.IdDuLieuHoatDongNavigation).WithOne(p => p.KetQuaTinhToan).HasConstraintName("ket_qua_tinh_toan_id_du_lieu_hoat_dong_fkey");

            entity.HasOne(d => d.IdHeSoPhatThaiNavigation).WithMany(p => p.KetQuaTinhToans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ket_qua_tinh_toan_id_he_so_phat_thai_fkey");

            entity.HasOne(d => d.IdKyBaoCaoNavigation).WithMany(p => p.KetQuaTinhToans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ket_qua_tinh_toan_id_ky_bao_cao_fkey");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.KetQuaTinhToans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ket_qua_tinh_toan_id_to_chuc_fkey");
        });

        modelBuilder.Entity<KyBaoCao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ky_bao_cao_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.TrangThaiKhoaSo).HasDefaultValueSql("'mo_so'::character varying");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.KyBaoCaos).HasConstraintName("ky_bao_cao_id_to_chuc_fkey");
        });

        modelBuilder.Entity<LichSuPheDuyet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lich_su_phe_duyet_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.NgayThucHien).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdDuLieuHoatDongNavigation).WithMany(p => p.LichSuPheDuyets).HasConstraintName("lich_su_phe_duyet_id_du_lieu_hoat_dong_fkey");

            entity.HasOne(d => d.IdNguoiThucHienNavigation).WithMany(p => p.LichSuPheDuyets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lich_su_phe_duyet_id_nguoi_thuc_hien_fkey");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nguoi_dung_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.TrangThai).HasDefaultValueSql("'hoat_dong'::character varying");
            entity.Property(e => e.DuongDanChuKyAnh).HasMaxLength(500).HasColumnName("duong_dan_chu_ky_anh");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.NguoiDungs)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nguoi_dung_id_to_chuc_fkey");

            entity.HasOne(d => d.IdVaiTroNavigation).WithMany(p => p.NguoiDungs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nguoi_dung_id_vai_tro_fkey");
        });

        modelBuilder.Entity<ThongBaoCanhBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("thong_bao_canh_bao_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.DaXuLy).HasDefaultValue(false);
            entity.Property(e => e.NgayPhatHien).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdDinhMucNavigation).WithMany(p => p.ThongBaoCanhBaos).HasConstraintName("thong_bao_canh_bao_id_dinh_muc_fkey");

            entity.HasOne(d => d.IdKyBaoCaoNavigation).WithMany(p => p.ThongBaoCanhBaos).HasConstraintName("thong_bao_canh_bao_id_ky_bao_cao_fkey");

            entity.HasOne(d => d.IdToChucNavigation).WithMany(p => p.ThongBaoCanhBaos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("thong_bao_canh_bao_id_to_chuc_fkey");
        });

        modelBuilder.Entity<ToChuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("to_chuc_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.TrangThai).HasDefaultValueSql("'hoat_dong'::character varying");

            entity.HasOne(d => d.IdToChucChaNavigation).WithMany(p => p.InverseIdToChucChaNavigation)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("to_chuc_id_to_chuc_cha_fkey");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vai_tro_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
