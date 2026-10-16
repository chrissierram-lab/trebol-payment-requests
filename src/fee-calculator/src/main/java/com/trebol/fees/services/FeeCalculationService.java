package com.trebol.fees.services;

import com.trebol.fees.contracts.FeeCalculationRequest;
import com.trebol.fees.contracts.FeeCalculationResponse;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.Map;
import org.springframework.stereotype.Service;

@Service
public class FeeCalculationService {
    private static final Map<String, BigDecimal> RATES = Map.of(
            "WEB", new BigDecimal("0.01"),
            "MOBILE", new BigDecimal("0.005"),
            "BRANCH", new BigDecimal("0.015"));

    public FeeCalculationResponse calculate(FeeCalculationRequest request) {
        BigDecimal rate = RATES.get(request.channel());
        BigDecimal fee = request.amount()
                .multiply(rate)
                .setScale(2, RoundingMode.HALF_UP);
        BigDecimal total = request.amount()
                .add(fee)
                .setScale(2, RoundingMode.HALF_UP);

        return new FeeCalculationResponse(fee, total);
    }
}