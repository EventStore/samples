package com.example.esdbsamplespringboot;

import com.google.gson.Gson;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

@SpringBootApplication
public class HelloWorldApplication {
    public static void main(String[] args) {
        SpringApplication.run(HelloWorldApplication.class, args);
    }

    @Bean
    public Gson GsonMapper() {
        return new Gson();
    }
}
