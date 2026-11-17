using CEMIS.Models;
using Microsoft.EntityFrameworkCore;

namespace CEMIS.Common;

public static class DataSeeder
{
    public static async Task ApplyMigrationsAsync(CemisDbContext context)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE nguoi_dung ADD COLUMN IF NOT EXISTS duong_dan_chu_ky_anh VARCHAR(500) NULL;"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Migration] Error applying migration: {ex.Message}");
        }
    }

    public static async Task SeedScope3Async(CemisDbContext context)
    {
        try
        {
            var gwp = await context.HeSoGwps.FirstOrDefaultAsync(g => g.PhienBanIpcc == "AR5") 
                      ?? await context.HeSoGwps.FirstOrDefaultAsync();

            long? gwpId = gwp?.Id;

            // 1. Công tác máy bay
            var mayBay = await context.DanhMucNguonPhatThais.FirstOrDefaultAsync(n => n.MaNguon == "CONG_TAC_MAY_BAY");
            if (mayBay == null)
            {
                mayBay = new DanhMucNguonPhatThai
                {
                    MaNguon = "CONG_TAC_MAY_BAY",
                    TenNguon = "Công tác bằng máy bay (Hàng không nội địa / quốc tế)",
                    PhamVi = "scope_3",
                    PhanLoai = "cong_tac",
                    DonViTinhChuan = "p.km",
                    MoTa = "Phát thải gián tiếp từ các chuyến bay công tác của cán bộ nhân viên"
                };
                context.DanhMucNguonPhatThais.Add(mayBay);
                await context.SaveChangesAsync();

                context.HeSoPhatThais.Add(new HeSoPhatThai
                {
                    IdNguonPhatThai = mayBay.Id,
                    IdGwp = gwpId,
                    TenHeSo = "Hệ số phát thải hàng không (DEFRA / ICAO)",
                    NguonGoc = "UK DEFRA 2022 / ICAO Carbon Calculator",
                    NamApDung = 2026,
                    HeSoCo2 = 0.148m,
                    HeSoCh4 = 0.0001m,
                    HeSoN2o = 0.00002m,
                    TongHeSoCo2e = 0.150m,
                    DonViHeSo = "kgCO2e/p.km",
                    ApDung = true
                });
                await context.SaveChangesAsync();
            }

            // 2. Công tác xe khách / xe thuê ngoài
            var xeKhach = await context.DanhMucNguonPhatThais.FirstOrDefaultAsync(n => n.MaNguon == "CONG_TAC_XE_KHACH");
            if (xeKhach == null)
            {
                xeKhach = new DanhMucNguonPhatThai
                {
                    MaNguon = "CONG_TAC_XE_KHACH",
                    TenNguon = "Công tác bằng xe khách / xe thuê ngoài",
                    PhamVi = "scope_3",
                    PhanLoai = "cong_tac",
                    DonViTinhChuan = "p.km",
                    MoTa = "Phát thải gián tiếp từ di chuyển xe tuyến, xe hợp đồng thuê ngoài"
                };
                context.DanhMucNguonPhatThais.Add(xeKhach);
                await context.SaveChangesAsync();

                context.HeSoPhatThais.Add(new HeSoPhatThai
                {
                    IdNguonPhatThai = xeKhach.Id,
                    IdGwp = gwpId,
                    TenHeSo = "Hệ số phát thải xe khách / vận tải đường bộ",
                    NguonGoc = "UK DEFRA 2022 Guidelines",
                    NamApDung = 2026,
                    HeSoCo2 = 0.088m,
                    HeSoCh4 = 0.00003m,
                    HeSoN2o = 0.00001m,
                    TongHeSoCo2e = 0.089m,
                    DonViHeSo = "kgCO2e/p.km",
                    ApDung = true
                });
                await context.SaveChangesAsync();
            }

            // 3. Rác thải sinh hoạt chôn lấp
            var racThai = await context.DanhMucNguonPhatThais.FirstOrDefaultAsync(n => n.MaNguon == "RAC_THAI_CHON_LAP");
            if (racThai == null)
            {
                racThai = new DanhMucNguonPhatThai
                {
                    MaNguon = "RAC_THAI_CHON_LAP",
                    TenNguon = "Rác thải sinh hoạt chôn lấp tập trung",
                    PhamVi = "scope_3",
                    PhanLoai = "chat_thai",
                    DonViTinhChuan = "tan",
                    MoTa = "Phát thải khí methane CH4 từ quá trình phân hủy kỵ khí rác thải tại bãi chôn lấp"
                };
                context.DanhMucNguonPhatThais.Add(racThai);
                await context.SaveChangesAsync();

                context.HeSoPhatThais.Add(new HeSoPhatThai
                {
                    IdNguonPhatThai = racThai.Id,
                    IdGwp = gwpId,
                    TenHeSo = "Hệ số phát thải bãi chôn lấp chất thải rắn",
                    NguonGoc = "IPCC 2006 Waste Model / MoNRE",
                    NamApDung = 2026,
                    HeSoCo2 = 0m,
                    HeSoCh4 = 16.07m, // 16.07 kg CH4 * 28 = ~450 kgCO2e
                    HeSoN2o = 0m,
                    TongHeSoCo2e = 450.0m,
                    DonViHeSo = "kgCO2e/tan",
                    ApDung = true
                });
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DataSeeder] SeedScope3Async note: {ex.Message}");
        }
    }
}
