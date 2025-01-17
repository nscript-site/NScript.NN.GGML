﻿using NScript.NN.GGML;
using System.Diagnostics;
using static NScript.NN.GGML.Ggml;

unsafe
{
    ggml_init_params init_params = default;
    {
        init_params.mem_size = 128 * 1024 * 1024;
        init_params.mem_buffer = null;
        init_params.no_alloc = false;
    };

    ggml_context* ctx0 = ggml_init(init_params);

    {
        ggml_tensor *x = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);

        ggml_set_param(ctx0, x);

        ggml_tensor *a = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);
        ggml_tensor *b = ggml_mul(ctx0, x, x);
        ggml_tensor *f = ggml_mul(ctx0, b, a);

        // a*x^2
        // 2*a*x

        ggml_print_objects(ctx0);

        ggml_cgraph gf = ggml_build_forward(f);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_set_f32(x, 2.0f);
        ggml_set_f32(a, 3.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(f->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("f     = {0:F6}", ggml_get_f32_1d(f, 0));
        Console.WriteLine("df/dx = {0:F6}", ggml_get_f32_1d(x->grad, 0));

        Debug.Assert(ggml_get_f32_1d(f, 0) == 12.0f);
        Debug.Assert(ggml_get_f32_1d(x->grad, 0) == 12.0f);

        ggml_set_f32(x, 3.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(f->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("f     = {0:F6}", ggml_get_f32_1d(f, 0));
        Console.WriteLine("df/dx = {0:F6}", ggml_get_f32_1d(x->grad, 0));

        Debug.Assert(ggml_get_f32_1d(f, 0) == 27.0f);
        Debug.Assert(ggml_get_f32_1d(x->grad, 0) == 18.0f);

        ggml_graph_dump_dot(&gf, null, "test1-1-forward.dot");
        ggml_graph_dump_dot(&gb, &gf, "test1-1-backward.dot");
    }

    ///////////////////////////////////////////////////////////////

    {
        ggml_tensor * x1 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);
        ggml_tensor * x2 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);
        ggml_tensor * x3 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);

        ggml_set_f32(x1, 3.0f);
        ggml_set_f32(x2, 1.0f);
        ggml_set_f32(x3, 0.0f);

        ggml_set_param(ctx0, x1);
        ggml_set_param(ctx0, x2);

        ggml_tensor * y = ggml_add(ctx0, ggml_mul(ctx0, x1, x1), ggml_mul(ctx0, x1, x2));

        ggml_cgraph gf = ggml_build_forward(y);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6}", ggml_get_f32_1d(x1->grad, 0));
        Console.WriteLine("df/dx2 = {0:F6}", ggml_get_f32_1d(x2->grad, 0));

        Debug.Assert(ggml_get_f32_1d(y, 0)        == 12.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 7.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 3.0f);

        ggml_tensor * g1 = x1->grad;
        ggml_tensor * g2 = x2->grad;

        ggml_cgraph gbb = ggml_build_backward(ctx0, &gb, true);

        ggml_graph_reset(&gb);
        ggml_set_f32(g1->grad, 1.0f);
        ggml_set_f32(g2->grad, 1.0f);

        ggml_graph_compute(ctx0, &gbb);

        Console.WriteLine("H * [1, 1] = [ {0:F6} {1:F6} ]\n", ggml_get_f32_1d(x1->grad, 0), ggml_get_f32_1d(x2->grad, 0));

        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 3.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 1.0f);

        ggml_graph_dump_dot(&gf, null, "test1-2-forward.dot");
        ggml_graph_dump_dot(&gb, &gf,  "test1-2-backward.dot");
    }

    ///////////////////////////////////////////////////////////////

    {
        ggml_tensor* x1 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);
        ggml_tensor* x2 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);

        ggml_set_param(ctx0, x1);
        ggml_set_param(ctx0, x2);

        ggml_tensor* y = ggml_mul(ctx0, ggml_add(ctx0, ggml_mul(ctx0, x1, x1), ggml_mul(ctx0, x1, x2)), x1);

        ggml_cgraph gf = ggml_build_forward(y);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_set_f32(x1, 3.0f);
        ggml_set_f32(x2, 4.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6}", ggml_get_f32_1d(x1->grad, 0));
        Console.WriteLine("df/dx2 = {0:F6}", ggml_get_f32_1d(x2->grad, 0));

        Debug.Assert(ggml_get_f32_1d(y, 0) == 63.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 51.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 9.0f);

        ggml_graph_dump_dot(&gf, null, "test1-3-forward.dot");
        ggml_graph_dump_dot(&gb, &gf, "test1-3-backward.dot");
    }

    ///////////////////////////////////////////////////////////////

    {
        ggml_tensor* x1 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);
        ggml_tensor* x2 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);
        ggml_tensor* x3 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 1);

        ggml_set_param(ctx0, x1);
        ggml_set_param(ctx0, x2);
        ggml_set_param(ctx0, x3);

        ggml_tensor* y = ggml_mul(ctx0, ggml_mul(ctx0, ggml_mul(ctx0, x1, x1), ggml_mul(ctx0, x2, x2)), x3);

        ggml_cgraph gf = ggml_build_forward(y);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_set_f32(x1, 1.0f);
        ggml_set_f32(x2, 2.0f);
        ggml_set_f32(x3, 3.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6}", ggml_get_f32_1d(x1->grad, 0));
        Console.WriteLine("df/dx2 = {0:F6}", ggml_get_f32_1d(x2->grad, 0));
        Console.WriteLine("df/dx3 = {0:F6}", ggml_get_f32_1d(x3->grad, 0));

        Debug.Assert(ggml_get_f32_1d(y, 0) == 12.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 24.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 12.0f);
        Debug.Assert(ggml_get_f32_1d(x3->grad, 0) == 4.0f);

        ggml_tensor* g1 = x1->grad;
        ggml_tensor* g2 = x2->grad;
        ggml_tensor* g3 = x3->grad;

        ggml_cgraph gbb = ggml_build_backward(ctx0, &gb, true);

        ggml_graph_reset(&gb);
        ggml_set_f32(g1->grad, 1.0f);
        ggml_set_f32(g2->grad, 1.0f);
        ggml_set_f32(g3->grad, 1.0f);

        ggml_graph_compute(ctx0, &gbb);

        Console.WriteLine("H * [1, 1, 1] = [ {0:F6} {1:F6} {2:F6} ]\n",
                ggml_get_f32_1d(x1->grad, 0),
                ggml_get_f32_1d(x2->grad, 0),
                ggml_get_f32_1d(x3->grad, 0));

        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 56.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 34.0f);
        Debug.Assert(ggml_get_f32_1d(x3->grad, 0) == 12.0f);

        ggml_graph_dump_dot(&gf, null, "test1-4-forward.dot");
        ggml_graph_dump_dot(&gb, &gf, "test1-4-backward.dot");
    }

    ///////////////////////////////////////////////////////////////

    {
        ggml_tensor * x1 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);
        ggml_tensor * x2 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);

        ggml_set_param(ctx0, x1);
        ggml_set_param(ctx0, x2);

        ggml_tensor * y = ggml_sum(ctx0, ggml_mul(ctx0, x1, x2));

        ggml_cgraph gf = ggml_build_forward(y);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_set_f32(x1, 3.0f);
        ggml_set_f32(x2, 5.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6} {1:F6} {2:F6}",
            ggml_get_f32_1d(x1->grad, 0),
            ggml_get_f32_1d(x1->grad, 1),
            ggml_get_f32_1d(x1->grad, 2));
        Console.WriteLine("df/dx2 = {0:F6} {1:F6} {2:F6}",
            ggml_get_f32_1d(x2->grad, 0),
            ggml_get_f32_1d(x2->grad, 1),
            ggml_get_f32_1d(x2->grad, 2));

        Debug.Assert(ggml_get_f32_1d(y, 0)        == 45.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 5.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 3.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 1) == 5.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 1) == 3.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 2) == 5.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 2) == 3.0f);

        ggml_graph_dump_dot(&gf, null, "test1-5-forward.dot");
        ggml_graph_dump_dot(&gb, &gf,  "test1-5-backward.dot");
    }

    ///////////////////////////////////////////////////////////////

    {
        ggml_tensor* x1 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);
        ggml_tensor* x2 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);

        ggml_set_param(ctx0, x1);
        ggml_set_param(ctx0, x2);

        ggml_tensor* y =
            ggml_sum(ctx0,
                    ggml_add(ctx0,
                        ggml_mul(ctx0, x1, x2),
                        ggml_mul(ctx0,
                            ggml_repeat(ctx0, ggml_new_f32(ctx0, -2.0f), x1),
                            ggml_mul(ctx0, x1, x1)
                            )
                        )
                    );

        ggml_cgraph gf = ggml_build_forward(y);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_set_f32(x1, 3.0f);
        ggml_set_f32(x2, 5.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x1->grad, 0),
                ggml_get_f32_1d(x1->grad, 1),
                ggml_get_f32_1d(x1->grad, 2));
        Console.WriteLine("df/dx2 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x2->grad, 0),
                ggml_get_f32_1d(x2->grad, 1),
                ggml_get_f32_1d(x2->grad, 2));

        Debug.Assert(ggml_get_f32_1d(y, 0) == -9.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == -7.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 1) == -7.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 2) == -7.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 3.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 1) == 3.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 2) == 3.0f);

        ggml_graph_dump_dot(&gf, null, "test1-6-forward.dot");
        ggml_graph_dump_dot(&gb, &gf, "test1-6-backward.dot");
    }

    ///////////////////////////////////////////////////////////////

    {
        ggml_tensor * x1 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);
        ggml_tensor * x2 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);

        ggml_set_param(ctx0, x1);
        ggml_set_param(ctx0, x2);

        ggml_tensor * y =
            ggml_sum(ctx0,
                    ggml_sub(ctx0,
                        ggml_mul(ctx0, x1, x2),
                        ggml_mul(ctx0,
                            ggml_mul(ctx0, x1, x1),
                            ggml_repeat(ctx0, ggml_new_f32(ctx0, -2.0f), x1)
                            )
                        )
                    );

        ggml_cgraph gf = ggml_build_forward(y);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_set_f32(x1, 3.0f);
        ggml_set_f32(x2, 5.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x1->grad, 0),
                ggml_get_f32_1d(x1->grad, 1),
                ggml_get_f32_1d(x1->grad, 2));
        Console.WriteLine("df/dx2 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x2->grad, 0),
                ggml_get_f32_1d(x2->grad, 1),
                ggml_get_f32_1d(x2->grad, 2));

        Debug.Assert(ggml_get_f32_1d(y, 0)        == 99.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 17.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 1) == 17.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 2) == 17.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) ==  3.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 1) ==  3.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 2) ==  3.0f);

        ggml_graph_dump_dot(&gf, null, "test1-7-forward.dot");
        ggml_graph_dump_dot(&gb, &gf,  "test1-7-backward.dot");
    }

    ///////////////////////////////////////////////////////////////

    {
        ggml_tensor* x1 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);
        ggml_tensor* x2 = ggml_new_tensor_1d(ctx0, ggml_type.GGML_TYPE_F32, 3);

        ggml_set_param(ctx0, x1);
        ggml_set_param(ctx0, x2);

        ggml_tensor* y =
            ggml_abs(ctx0,
                    ggml_sub(ctx0, x1, x2)
                    );

        ggml_cgraph gf = ggml_build_forward(y);
        ggml_cgraph gb = ggml_build_backward(ctx0, &gf, false);

        ggml_set_f32(x1, 3.0f);
        ggml_set_f32(x2, 5.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x1->grad, 0),
                ggml_get_f32_1d(x1->grad, 1),
                ggml_get_f32_1d(x1->grad, 2));
        Console.WriteLine("df/dx2 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x2->grad, 0),
                ggml_get_f32_1d(x2->grad, 1),
                ggml_get_f32_1d(x2->grad, 2));

        Debug.Assert(ggml_get_f32_1d(y, 0) == 2.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == -1.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 1) == -1.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 2) == -1.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == 1.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 1) == 1.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 2) == 1.0f);

        ggml_set_f32(x1, 7.0f);
        ggml_set_f32(x2, 5.0f);

        ggml_graph_reset(&gf);
        ggml_set_f32(y->grad, 1.0f);

        ggml_graph_compute(ctx0, &gb);

        Console.WriteLine("y      = {0:F6}", ggml_get_f32_1d(y, 0));
        Console.WriteLine("df/dx1 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x1->grad, 0),
                ggml_get_f32_1d(x1->grad, 1),
                ggml_get_f32_1d(x1->grad, 2));
        Console.WriteLine("df/dx2 = {0:F6} {1:F6} {2:F6}",
                ggml_get_f32_1d(x2->grad, 0),
                ggml_get_f32_1d(x2->grad, 1),
                ggml_get_f32_1d(x2->grad, 2));

        Debug.Assert(ggml_get_f32_1d(y, 0) == 2.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 0) == 1.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 1) == 1.0f);
        Debug.Assert(ggml_get_f32_1d(x1->grad, 2) == 1.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 0) == -1.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 1) == -1.0f);
        Debug.Assert(ggml_get_f32_1d(x2->grad, 2) == -1.0f);

        ggml_graph_dump_dot(&gf, null, "test1-8-forward.dot");
        ggml_graph_dump_dot(&gb, &gf, "test1-8-backward.dot");
    }

    ggml_free(ctx0);

    return 0;
}