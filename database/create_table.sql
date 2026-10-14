-- Reference schema reconstructed from the application's queries, for local development.
-- Lengths and precisions are reasonable defaults, not a copy of a production DDL.
--
-- Target table for imported production order lines.
-- Column names follow the original integration (N_ZAK = order number, ART = product code,
-- ART_MAT = material code, ...); see OrderLine.cs for the mapping to English names.

CREATE TABLE A_FROM_XML_ZAK (
    N_ZAK       VARCHAR2(50)   NOT NULL,  -- order number
    DATE_OTG    VARCHAR2(30),             -- shipment date, as exported by 1C
    QTY         NUMBER(18, 3)  NOT NULL,  -- ordered quantity
    ART         VARCHAR2(100)  NOT NULL,  -- product code
    NAME        VARCHAR2(500),            -- product name
    QTY_NA_IZD  NUMBER(18, 3),            -- material quantity per product
    ART_MAT     VARCHAR2(100),            -- material code
    NAME_MAT    VARCHAR2(500),            -- material name
    ED_IZM      VARCHAR2(30),             -- unit of measure
    ROAT        VARCHAR2(50),             -- rope length
    MARSHRUT    VARCHAR2(500),            -- production route
    GROUP_ITEM  VARCHAR2(200),            -- product group
    CLASS_ITEM  VARCHAR2(200),            -- product class
    UPAK        VARCHAR2(10),             -- packaging flag
    QTY_OTG     NUMBER(18, 3),            -- shipped quantity
    PVZ         VARCHAR2(100)             -- consolidated production task
);
