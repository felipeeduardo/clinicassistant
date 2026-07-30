\set profile tenant
\if :{?tenant_id}
\else
  \quit 3
\endif
\ir reset_test_data.sql
