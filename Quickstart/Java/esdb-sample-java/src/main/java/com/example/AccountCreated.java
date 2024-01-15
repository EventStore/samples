package com.example;

import java.util.UUID;

class AccountCreated {
    private UUID id;
    private String login;

    public UUID getId() {
        return id;
    }

    public String getLogin() {
        return login;
    }

    public void setId(UUID id) {
        this.id = id;
    }

    public void setLogin(String login) {
        this.login = login;
    }
}
