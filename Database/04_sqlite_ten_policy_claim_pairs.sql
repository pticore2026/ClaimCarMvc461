-- 10 hợp đồng và 10 hồ sơ bồi thường tương ứng dành cho SQLite.
-- Có thể chạy lại an toàn nhờ các khóa duy nhất và INSERT OR IGNORE.
BEGIN TRANSACTION;

WITH RECURSIVE numbers(n) AS (
    SELECT 1 UNION ALL SELECT n + 1 FROM numbers WHERE n < 10
)
INSERT OR IGNORE INTO VEHICLE_POLICY (
    SO_HOP_DONG, SO_DON_BAO_HIEM, MA_DON_VI, NGAY_CAP_DON,
    MA_KHACH_HANG, TEN_CHU_XE, LOAI_KHACH_HANG, SO_GIAY_TO,
    DIEN_THOAI, EMAIL, DIA_CHI, BIEN_SO, SO_KHUNG, SO_MAY,
    NHAN_HIEU, DONG_XE, NAM_SAN_XUAT, MUC_DICH_SU_DUNG, SO_CHO,
    HIEU_LUC_TU, HIEU_LUC_DEN, PHAM_VI_BAO_HIEM, NGOAI_TE,
    GIA_TRI_XE, SO_TIEN_BAO_HIEM, PHI_TRUOC_THUE, THUE_GTGT,
    TONG_PHI, MUC_KHAU_TRU, KENH_KHAI_THAC, MA_DAI_LY,
    CAN_BO_CAP_DON, TRANG_THAI, GHI_CHU
)
SELECT
    printf('HD-2026-%04d', n), printf('GCN-2026-%04d', n),
    printf('%03d', ((n - 1) % 3) + 1), printf('2026-%02d-05', n),
    printf('KH%05d', n),
    CASE n
        WHEN 1 THEN 'Nguyễn Minh Anh' WHEN 2 THEN 'Trần Thu Hà'
        WHEN 3 THEN 'Lê Quốc Bảo' WHEN 4 THEN 'Phạm Hoàng Long'
        WHEN 5 THEN 'Vũ Ngọc Mai' WHEN 6 THEN 'Đặng Thành Nam'
        WHEN 7 THEN 'Bùi Hải Yến' WHEN 8 THEN 'Đỗ Đức Huy'
        WHEN 9 THEN 'Hồ Thanh Trúc' ELSE 'Ngô Quang Vinh' END,
    CASE WHEN n IN (4, 8) THEN 'DOANH_NGHIEP' ELSE 'CA_NHAN' END,
    printf('07920600%04d', n), printf('0908%06d', n),
    printf('khachhang%02d@example.com', n), printf('%d Nguyễn Văn Linh, TP Hồ Chí Minh', n * 12),
    CASE n
        WHEN 1 THEN '51A-101.01' WHEN 2 THEN '51B-202.02'
        WHEN 3 THEN '30A-303.03' WHEN 4 THEN '43A-404.04'
        WHEN 5 THEN '92A-505.05' WHEN 6 THEN '60A-606.06'
        WHEN 7 THEN '61A-707.07' WHEN 8 THEN '29A-808.08'
        WHEN 9 THEN '15A-909.09' ELSE '36A-110.10' END,
    printf('RLV2026DEMO%06d', n), printf('ENG2026%06d', n),
    CASE ((n - 1) % 5)
        WHEN 0 THEN 'Toyota' WHEN 1 THEN 'Honda' WHEN 2 THEN 'Ford'
        WHEN 3 THEN 'Mazda' ELSE 'Hyundai' END,
    CASE ((n - 1) % 5)
        WHEN 0 THEN 'Vios' WHEN 1 THEN 'City' WHEN 2 THEN 'Ranger'
        WHEN 3 THEN 'CX-5' ELSE 'Accent' END,
    2017 + n, CASE WHEN n % 3 = 0 THEN 'Kinh doanh' ELSE 'Cá nhân' END,
    CASE WHEN n % 3 = 0 THEN 7 ELSE 5 END,
    '2026-01-01', '2026-12-31', 'Bảo hiểm vật chất xe toàn diện', 'VND',
    450000000 + n * 50000000, 450000000 + n * 50000000,
    4500000 + n * 500000, (4500000 + n * 500000) * 0.1,
    (4500000 + n * 500000) * 1.1, 500000 + n * 100000,
    CASE WHEN n % 2 = 0 THEN 'Đại lý' ELSE 'Trực tiếp' END,
    printf('DL%03d', n), printf('CB%03d', n), 'DA_CAP',
    printf('Hợp đồng mẫu số %d phục vụ kiểm thử hồ sơ bồi thường.', n)
FROM numbers;

WITH RECURSIVE numbers(n) AS (
    SELECT 1 UNION ALL SELECT n + 1 FROM numbers WHERE n < 10
)
INSERT OR IGNORE INTO CLAIM_GENERAL (
    MA_DON_VI, TEN_DON_VI, MA_KHU_VUC, TEN_KHU_VUC, BIEN_SO,
    NGAY_NHAP, NGAY_NHAP_CALL, SO_HOP_DONG, TINH_TRANG,
    NGAY_QUYET_DINH, NGAY_XAY_RA, NGAY_THONG_BAO,
    SO_HO_SO, MA_GDV, GIA_TRI_BH
)
SELECT
    printf('%03d', ((n - 1) % 3) + 1),
    CASE ((n - 1) % 3) WHEN 0 THEN 'TP Hồ Chí Minh' WHEN 1 THEN 'Hà Nội' ELSE 'Đà Nẵng' END,
    CASE ((n - 1) % 3) WHEN 0 THEN 'HCM' WHEN 1 THEN 'HN' ELSE 'DNG' END,
    CASE ((n - 1) % 3) WHEN 0 THEN 'Khu vực TP Hồ Chí Minh' WHEN 1 THEN 'Khu vực Hà Nội' ELSE 'Khu vực Đà Nẵng' END,
    CASE n
        WHEN 1 THEN '51A-101.01' WHEN 2 THEN '51B-202.02'
        WHEN 3 THEN '30A-303.03' WHEN 4 THEN '43A-404.04'
        WHEN 5 THEN '92A-505.05' WHEN 6 THEN '60A-606.06'
        WHEN 7 THEN '61A-707.07' WHEN 8 THEN '29A-808.08'
        WHEN 9 THEN '15A-909.09' ELSE '36A-110.10' END,
    printf('2026-08-%02d', 10 + n), printf('2026-08-%02d', 10 + n),
    printf('HD-2026-%04d', n),
    CASE ((n - 1) % 5)
        WHEN 0 THEN 'Mới tiếp nhận' WHEN 1 THEN 'Đang giám định'
        WHEN 2 THEN 'Đã bảo lãnh' WHEN 3 THEN 'Đã trình duyệt' ELSE 'Đã duyệt' END,
    CASE WHEN n >= 8 THEN printf('2026-08-%02d', 11 + n) ELSE NULL END,
    printf('2026-08-%02d', 9 + n), printf('2026-08-%02d', 10 + n),
    printf('HSBT-2026-%04d', n), printf('GDV%03d', n),
    450000000 + n * 50000000
FROM numbers;

COMMIT;
