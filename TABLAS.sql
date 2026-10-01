CREATE TABLE users (
                       id BIGSERIAL PRIMARY KEY,
                       uuid VARCHAR(36) NOT NULL UNIQUE,
                       username VARCHAR(16) NOT NULL
);
CREATE INDEX idx_users_username ON users (username);

CREATE TABLE ranks (
                       id SERIAL PRIMARY KEY,
                       name VARCHAR(50) NOT NULL,
                       price NUMERIC(10, 2) NOT NULL,
                       server_rank VARCHAR(32) NOT NULL,
                       description TEXT NULL
);

CREATE TABLE prefixes (
                          id SERIAL PRIMARY KEY,
                          name VARCHAR(50) NOT NULL,
                          price NUMERIC(10, 2) NOT NULL,
                          server_tag VARCHAR(32) NOT NULL
);

CREATE TABLE consumables (
                             id SERIAL PRIMARY KEY,
                             name VARCHAR(50) NOT NULL,
                             price NUMERIC(10, 2) NOT NULL,
                             server_item VARCHAR(32) NOT NULL,
                             quantity INT NOT NULL DEFAULT 1
);

CREATE TABLE appeals (
                         id SERIAL PRIMARY KEY,
                         name VARCHAR(50) NOT NULL,
                         price NUMERIC(10, 2) NOT NULL,
                         server_appeal VARCHAR(32) NOT NULL
);

CREATE TABLE orders (
                        id BIGSERIAL PRIMARY KEY,
                        order_code VARCHAR(32) NOT NULL UNIQUE,
                        user_id BIGINT NOT NULL REFERENCES users(id),
                        total_amount NUMERIC(10, 2) NOT NULL,
                        gateway_payment_id VARCHAR(128) NULL,
                        status VARCHAR(32) NOT NULL DEFAULT 'PENDING',
                        created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX idx_orders_user_id ON orders (user_id);
CREATE INDEX idx_orders_order_code ON orders (order_code);

CREATE TABLE order_items (
                             id BIGSERIAL PRIMARY KEY,
                             order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
                             product_category VARCHAR(32) NOT NULL,
                             product_internal_name VARCHAR(64) NOT NULL,
                             product_name VARCHAR(100) NOT NULL,
                             unit_price NUMERIC(10, 2) NOT NULL,
                             quantity INT NOT NULL DEFAULT 1,
                             custom_input TEXT NULL
);
CREATE INDEX idx_order_items_order_id ON order_items (order_id);

-- SERVER

CREATE TABLE user_ranks (
                            id BIGSERIAL PRIMARY KEY,
                            uuid VARCHAR(36) NOT NULL,
                            server_rank VARCHAR(32) NOT NULL,
                            acquired_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            CONSTRAINT uq_user_rank UNIQUE (uuid, server_rank)
);
CREATE INDEX idx_user_ranks_uuid ON user_ranks (uuid);

CREATE TABLE user_prefixes (
                               id BIGSERIAL PRIMARY KEY,
                               uuid VARCHAR(36) NOT NULL,
                               server_tag VARCHAR(32) NOT NULL,
                               acquired_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                               CONSTRAINT uq_user_prefix UNIQUE (uuid, server_tag)
);
CREATE INDEX idx_user_prefixes_uuid ON user_prefixes (uuid);

CREATE TABLE user_claims (
                             id BIGSERIAL PRIMARY KEY,
                             uuid VARCHAR(36) NOT NULL,
                             server_item VARCHAR(32) NOT NULL,
                             quantity INT NOT NULL DEFAULT 1,
                             is_claimed BOOLEAN NOT NULL DEFAULT FALSE,
                             claimed_at TIMESTAMPTZ NULL
);
CREATE INDEX idx_user_claims_pending ON user_claims (uuid, is_claimed);

CREATE TABLE user_appeals (
                              id BIGSERIAL PRIMARY KEY,
                              uuid VARCHAR(36) NOT NULL,
                              server_appeal VARCHAR(32) NOT NULL,
                              reason TEXT NULL,
                              is_processed BOOLEAN NOT NULL DEFAULT FALSE,
                              processed_at TIMESTAMPTZ NULL
);
CREATE INDEX idx_appeals_pending ON user_appeals (uuid, is_processed);


INSERT INTO ranks (name, price, server_rank, description) VALUES
                                                              ('Rango VIP', 5.00, 'vip', 'Acceso a /fly, 3 homes y kit VIP diario.'),
                                                              ('Rango MVP', 10.00, 'mvp', 'Acceso a /fly, /feed, 5 homes y kit MVP diario.'),
                                                              ('Rango ELITE', 15.00, 'elite', 'Beneficios anteriores mas kit ELITE y prioridad en cola.');

INSERT INTO prefixes (name, price, server_tag) VALUES
                                                   ('Tag Guerrero', 2.50, 'guerrero'),
                                                   ('Tag Leyenda', 3.50, 'leyenda'),
                                                   ('Tag Magister', 4.00, 'magister');

INSERT INTO consumables (name, price, server_item, quantity) VALUES
                                                                 ('Pack 5 Vidas HCF', 3.00, 'hcf_lives_5', 5),
                                                                 ('Pack 10 Vidas HCF', 5.50, 'hcf_lives_10', 10),
                                                                 ('3x Llaves Mythic', 4.00, 'mythic_key', 3);

INSERT INTO appeals (name, price, server_appeal) VALUES
                                                     ('Desbaneo Global', 15.00, 'unban'),
                                                     ('Desmute Global', 5.00, 'unmute');