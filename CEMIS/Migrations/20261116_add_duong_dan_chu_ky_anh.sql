-- Migration: Add nullable duong_dan_chu_ky_anh to nguoi_dung table
-- Compatible with existing data (defaults to NULL)
ALTER TABLE nguoi_dung 
ADD COLUMN IF NOT EXISTS duong_dan_chu_ky_anh VARCHAR(500) NULL;

COMMENT ON COLUMN nguoi_dung.duong_dan_chu_ky_anh IS 'Đường dẫn ảnh chữ ký tay của người dùng (PNG nền trong suốt)';
