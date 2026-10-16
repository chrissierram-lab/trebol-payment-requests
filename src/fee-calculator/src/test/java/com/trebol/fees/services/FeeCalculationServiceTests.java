package com.trebol.fees.services;

import static org.junit.jupiter.api.Assertions.assertEquals;

import com.trebol.fees.contracts.FeeCalculationRequest;
import com.trebol.fees.contracts.FeeCalculationResponse;
import java.math.BigDecimal;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;

class FeeCalculationServiceTests {
    private final FeeCalculationService service = new FeeCalculationService();

    @ParameterizedTest
    @CsvSource({
            "WEB, 1000.00, 10.00, 1010.00",
            "MOBILE, 1000.00, 5.00, 1005.00",
            "BRANCH, 1000.00, 15.00, 1015.00",
            "MOBILE, 1.00, 0.01, 1.01"
    })
    void calculatesFeeForEachChannelAndRoundsHalfUp(
            String channel,
            String amount,
            String expectedFee,
            String expectedTotal) {
        FeeCalculationResponse result = service.calculate(
                new FeeCalculationRequest(new BigDecimal(amount), channel));

        assertEquals(new BigDecimal(expectedFee), result.fee());
        assertEquals(new BigDecimal(expectedTotal), result.total());
    }
}