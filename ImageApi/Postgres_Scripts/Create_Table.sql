CREATE TABLE IF NOT EXISTS image_table (
    img_hash_full VARCHAR PRIMARY KEY,
    img_hash_partial VARCHAR(10) NOT NULL,
    img_data BYTEA NOT NULL
);