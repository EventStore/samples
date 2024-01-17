FROM golang:1.21-alpine as build
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN go build -o esdb-sample-go

FROM scratch
COPY --from=build /app/esdb-sample-go /
EXPOSE 8080
CMD ["/esdb-sample-go"]