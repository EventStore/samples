FROM rust:1.75
WORKDIR /usr/src/esdb-sample-rust
COPY . .
RUN cargo install --path .
EXPOSE 8000
ENTRYPOINT ["esdb-sample-rust"]