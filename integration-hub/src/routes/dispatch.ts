import { Router, Request, Response } from "express";
import express from "express";
import { getMockMESPayload } from "../services/payloadService";

const router = Router();

router.use(express.json());

router.get("/dispatch", (_req: Request, res: Response) => {
  try {
    const payload = getMockMESPayload();
    res.json(payload);
  } catch (err: any) {
    console.error(err);
    res.status(500).json({ error: err.message });
  }
});

export default router;