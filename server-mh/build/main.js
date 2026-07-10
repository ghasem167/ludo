"use strict";
function InitModule(ctx, logger, nk, initializer) {
    logger.info("INIT MODULE START");
    initializer.registerMatch("ludo", {
        matchInit: function (ctx, logger, nk, params) {
            logger.info("MATCH INIT");
            return {
                state: {},
                tickRate: 10,
                label: "ludo"
            };
        },
        matchJoinAttempt: function (ctx, logger, nk, dispatcher, tick, state, presence, metadata) {
            return {
                state,
                accept: true
            };
        },
        matchJoin: function (ctx, logger, nk, dispatcher, tick, state, presences) {
            return {
                state
            };
        },
        matchLeave: function (ctx, logger, nk, dispatcher, tick, state, presences) {
            return {
                state
            };
        },
        matchLoop: function (ctx, logger, nk, dispatcher, tick, state, messages) {
            return {
                state
            };
        },
        matchTerminate: function (ctx, logger, nk, dispatcher, tick, state, graceSeconds) {
            return {
                state
            };
        },
        matchSignal: function (ctx, logger, nk, dispatcher, tick, state, data) {
            return {
                state,
                data
            };
        }
    });
    logger.info("MATCH REGISTERED");
}
globalThis.InitModule = InitModule;
