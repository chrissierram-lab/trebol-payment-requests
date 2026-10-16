package com.trebol.fees.controllers;

import com.trebol.fees.contracts.FeeCalculationRequest;
import com.trebol.fees.contracts.FeeCalculationResponse;
import com.trebol.fees.services.FeeCalculationService;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/v1/fees")
public class FeeCalculationController {
    private final FeeCalculationService feeCalculationService;

    public FeeCalculationController(FeeCalculationService feeCalculationService) {
        this.feeCalculationService = feeCalculationService;
    }

    @PostMapping("/calculate")
    public ResponseEntity<FeeCalculationResponse> calculate(
            @Valid @RequestBody FeeCalculationRequest request) {
        return ResponseEntity.ok(feeCalculationService.calculate(request));
    }
}