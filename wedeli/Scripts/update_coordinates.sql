-- =====================================================
-- Update GPS Coordinates cho Transport Companies
-- Chạy script này để thêm tọa độ cho các nhà xe hiện có
-- =====================================================

-- Cập nhật tọa độ cho các nhà xe (ví dụ ở TP.HCM)
-- Thay đổi company_id và tọa độ phù hợp với dữ liệu của bạn

-- Nhà xe 1 (Thành Bưởi - Bến xe Miền Tây)
UPDATE transport_companies 
SET latitude = 10.7399,
    longitude = 106.6200
WHERE company_id = 1;

-- Nhà xe 2 (Phương Trang - Đề Thám)  
UPDATE transport_companies 
SET latitude = 10.7626,
    longitude = 106.6878
WHERE company_id = 2;

-- Nhà xe 3 (Hoàng Long - Bến xe Miền Đông)
UPDATE transport_companies 
SET latitude = 10.8142,
    longitude = 106.7107
WHERE company_id = 3;

-- Nếu có thêm nhà xe, thêm các UPDATE tương tự ở đây

-- Kiểm tra kết quả
SELECT company_id, company_name, latitude, longitude 
FROM transport_companies;
