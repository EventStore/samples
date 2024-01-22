FROM rust:1.75
WORKDIR /usr/src/esdb-sample-rust
COPY . .
RUN cargo install --path .
EXPOSE 8080
ENTRYPOINT ["esdb-sample-rust"]