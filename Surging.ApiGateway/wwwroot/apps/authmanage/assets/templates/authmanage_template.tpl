       {{each entity}}
	<tr>
                            <td class="center">
								  {{if ($index+1)%2==1 }}
                                  <span class="badge badge-grey ">${$index+1}</span>
                                  {{else}}
                                  <span class="badge   badge-success  ">${$index+1}</span>
                                  {{/if}}
                            </td>
                            <td class="center"> {{= [address.ip,address.port].join(":")}}</td>
                            <td class="center">已启动</td>
                            <td class="center">
							      {{if isHealth==true }}
								<span class="label label-success arrowed-in arrowed-in-right">正常</span>
								{{else}}
										<span class="label label-danger arrowed">异常</span>
								{{/if}}
							</td>
							 <td class="center">${address.token}</td>
							    <td class="center">
							      {{if address.disableAuth ==true }}
								<span class="label label-danger arrowed">是</span>
								{{else}}
										<span class="label label-success arrowed-in arrowed-in-right">否</span>
								{{/if}}
							</td>
                            <td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                                                   <a href="javascript:void(0);" class="red editServiceToken"><i class="icon-trash bigger-120"></i>编辑令牌</a>
                             </div></td>
                           
                        </tr>
						  {{/each}}