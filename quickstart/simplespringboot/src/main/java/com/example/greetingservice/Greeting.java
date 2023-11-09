package com.example.greetingservice;

import java.util.UUID;

public class Greeting {

	private UUID id;
	private String content;

	public UUID getId() {
		return id;
	}

    public void setId(UUID id) {
        this.id = id;
    }	

	public String getContent() {
		return content;
	}

    public void setContent(String content) {
        this.content = content;
    }	

}
