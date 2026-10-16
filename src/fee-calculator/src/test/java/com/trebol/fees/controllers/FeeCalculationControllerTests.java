package com.trebol.fees.controllers;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import com.trebol.fees.services.FeeCalculationService;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

class FeeCalculationControllerTests {
    private MockMvc mockMvc;

    @BeforeEach
    void setUp() {
        mockMvc = MockMvcBuilders.standaloneSetup(
                        new FeeCalculationController(new FeeCalculationService()))
                .setControllerAdvice(new ApiExceptionHandler())
                .build();
    }

    @Test
    void calculatesFeeForValidRequest() throws Exception {
        mockMvc.perform(post("/api/v1/fees/calculate")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"amount\":1.00,\"channel\":\"MOBILE\"}"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.fee").value(0.01))
                .andExpect(jsonPath("$.total").value(1.01));
    }

    @Test
    void returnsConsistentBadRequestForInvalidAmountAndChannel() throws Exception {
        mockMvc.perform(post("/api/v1/fees/calculate")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"amount\":0,\"channel\":\"ATM\"}"))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.code").value("INVALID_REQUEST"))
                .andExpect(jsonPath("$.message").isNotEmpty());
    }

    @Test
    void rejectsAmountWithMoreThanTwoDecimalPlaces() throws Exception {
        mockMvc.perform(post("/api/v1/fees/calculate")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"amount\":1.001,\"channel\":\"WEB\"}"))
                .andExpect(status().isBadRequest())
                .andExpect(jsonPath("$.code").value("INVALID_REQUEST"));
    }
}