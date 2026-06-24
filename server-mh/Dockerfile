FROM node:20-alpine AS node-builder

WORKDIR /backend

COPY package*.json ./
RUN npm install

COPY tsconfig.json ./
COPY src/ src/

RUN npx tsc


FROM registry.heroiclabs.com/heroiclabs/nakama:3.39.0

COPY --from=node-builder /backend/build/ /nakama/data/modules/build/

COPY local.yml /nakama/data/local.yml